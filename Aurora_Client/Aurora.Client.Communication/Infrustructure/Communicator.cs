using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Managers;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Infrustructure
{
    public class Communicator
    {
        private static readonly Lazy<Communicator> _instance = new(() => new Communicator());
        private readonly SemaphoreSlim _sendLock = new(1, 1); // Ensures only one thread writes at a time
        private readonly SemaphoreSlim _sendLockEncrypted = new(1, 1); // Ensures only one thread writes at a time
        private readonly SemaphoreSlim _readLock = new(1, 1); // Ensures only one thread writes at a time
        private readonly object _clientLock = new(); // Protects _client instance
        private readonly int CONNECTION_PORT = 1223;
        private readonly string CONNECTION_IP = "127.0.0.1";
        private TcpListener _listener;
        private TcpClient _serverConnect;
        private int _clientListenPort;
        private readonly int HEADER_SIZE = 5;

        public TcpClient Client { get; set; }
        public TcpClient ServerConnect { get; set; }

        public static Communicator Instance => _instance.Value;

        private Communicator()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string json = File.ReadAllText($"{appDataPath}/Aurora/appsettings.json");
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

            CONNECTION_IP = config["Server"]["IP"];
            Console.WriteLine($"Using port {_clientListenPort} for client listen.");
        }

        private TcpListener CreateAvailableListener(out int port)
        {
            int startPort = 1224;
            int maxPort = 65535;

            for (port = startPort; port <= maxPort; port++)
            {
                try
                {
                    var listener = new TcpListener(IPAddress.Loopback, port);
                    listener.Start(); // Keep it started
                    return listener;
                }
                catch (SocketException)
                {
                    continue;
                }
            }

            throw new Exception("No available ports found in the given range.");
        }

        public async Task<bool> ConnectToServerAsync()
        {
            lock (_clientLock)
            {
                if (Client?.Connected == true) return true; // Already connected
                Client = new TcpClient();
            }

            try
            {
                await Client.ConnectAsync(CONNECTION_IP, CONNECTION_PORT);
                _listener = CreateAvailableListener(out _clientListenPort);

                //send listen port to server
                RequestInfo requestInfo = new RequestInfo
                {
                    code = RequestCode.PORT_SEND_REQUEST_CODE,
                    message = _clientListenPort.ToString()
                };
                await SendMessageToServer(Client, requestInfo);
                var responseInfo = await ReadMessageFromServer(Client);

                ServerConnect = _listener.AcceptTcpClient();

                requestInfo = new RequestInfo
                {
                    code = RequestCode.GET_SERVER_RSA_PUBLIC_KEY_REQUEST_CODE,
                    message = ""
                };
                await SendMessageToServer(Client, requestInfo);
                responseInfo = await ReadMessageFromServer(Client);
                string pemPublicKey = responseInfo.message;

                // import RSA public key
                using var rsa = RSA.Create();
                rsa.ImportFromPem(pemPublicKey.ToCharArray());

                // generate AES key and IV
                using var aes = Aes.Create();
                aes.GenerateKey();
                aes.GenerateIV();

                // encrypt AES key and IV with RSA
                byte[] encryptedAesKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
                byte[] encryptedIv = rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1);

                // send encrypted AES key and IV to server
                var aesRequest = new RequestInfo
                {
                    code = RequestCode.SEND_AES_SETUP_REQUEST_CODE,
                    message = Newtonsoft.Json.JsonConvert.SerializeObject(new AesExchangeData
                    {
                        iv = encryptedIv,
                        key = encryptedAesKey
                    })
                };
                await SendMessageToServer(Client, aesRequest);
                responseInfo = await ReadMessageFromServer(Client);

                EncryptionManager.Instance.AesKey = aes.Key;
                EncryptionManager.Instance.AesIV = aes.IV;

                return true;


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Communication could not be established due to {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendMessageToServer(TcpClient client, RequestInfo info)
        {
            //await _sendLock.WaitAsync();
            try
            {
                if (client == null || !client.Connected)
                {
                    if (!await ConnectToServerAsync()) return false;
                }

                var messageBytes = Encoding.UTF8.GetBytes(info.message);
                var wholeMessage = new byte[HEADER_SIZE + messageBytes.Length];

                wholeMessage[0] = (byte)info.code;
                Array.Copy(BitConverter.GetBytes(messageBytes.Length), 0, wholeMessage, 1, 4);
                Array.Copy(messageBytes, 0, wholeMessage, HEADER_SIZE, messageBytes.Length);

                await client.GetStream().WriteAsync(wholeMessage, 0, wholeMessage.Length);
                await client.GetStream().FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
            finally
            {
                //_sendLock.Release(); // Always release lock
            }
        }

        public async Task<bool> SendMessageToServerEncrypted(TcpClient client, RequestInfo info)
        {
            //await _sendLockEncrypted.WaitAsync();
            try
            {
                if (client == null || !client.Connected)
                {
                    if (!await ConnectToServerAsync()) return false;
                }

                var messageBytes = await EncryptionManager.Instance.Encrypt(info);
                var wholeMessage = new byte[HEADER_SIZE + messageBytes.Length];

                wholeMessage[0] = (byte)info.code;
                Array.Copy(BitConverter.GetBytes(messageBytes.Length), 0, wholeMessage, 1, 4);
                Array.Copy(messageBytes, 0, wholeMessage, HEADER_SIZE, messageBytes.Length);

                await client.GetStream().WriteAsync(wholeMessage, 0, wholeMessage.Length);
                await client.GetStream().FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
            finally
            {
                //_sendLockEncrypted.Release(); // Always release lock
            }
        }

        public async Task<ResponseInfo> ReadMessageFromServerEncrypted(TcpClient client)
        {
            //await _readLock.WaitAsync();
            try
            {
                if (client == null || !client.Connected)
                {
                    throw new Exception("Client is disconnected.");
                }

                var headerMessage = new byte[HEADER_SIZE];
                int bytesRead = await client.GetStream().ReadAsync(headerMessage, 0, HEADER_SIZE);
                if (bytesRead < HEADER_SIZE) throw new Exception("Incomplete header received.");

                int messageSize = BitConverter.ToInt32(headerMessage, 1);
                var wholeMessage = new byte[messageSize];

                bytesRead = 0;
                while (bytesRead < messageSize)
                {
                    int read = await client.GetStream().ReadAsync(wholeMessage, bytesRead, messageSize - bytesRead);
                    if (read == 0) throw new Exception("Connection closed by server.");
                    bytesRead += read;
                }

                return new ResponseInfo
                {
                    code = (ResponseCode)headerMessage[0],
                    message = await EncryptionManager.Instance.Decrypt(wholeMessage)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading message: {ex.Message}");
                return new ResponseInfo { code = 0, message = "Failed to read message" };
            }
            finally
            {
                // _readLock.Release();
            }
        }

        public async Task<ResponseInfo> ReadMessageFromServer(TcpClient client)
        {
            await _readLock.WaitAsync();
            try
            {
                if (client == null || !client.Connected)
                {
                    throw new Exception("Client is disconnected.");
                }

                var headerMessage = new byte[HEADER_SIZE];
                int bytesRead = await client.GetStream().ReadAsync(headerMessage, 0, HEADER_SIZE);
                if (bytesRead < HEADER_SIZE) throw new Exception("Incomplete header received.");

                int messageSize = BitConverter.ToInt32(headerMessage, 1);
                var wholeMessage = new byte[messageSize];

                bytesRead = 0;
                while (bytesRead < messageSize)
                {
                    int read = await client.GetStream().ReadAsync(wholeMessage, bytesRead, messageSize - bytesRead);
                    if (read == 0) throw new Exception("Connection closed by server.");
                    bytesRead += read;
                }

                return new ResponseInfo
                {
                    code = (ResponseCode)headerMessage[0],
                    message = Encoding.UTF8.GetString(wholeMessage)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading message: {ex.Message}");
                return new ResponseInfo { code = 0, message = "Failed to read message" };
            }
            finally
            {
                _readLock.Release();
            }
        }

        public async Task<RequestInfo> ReadMessageFromServerUpdate(TcpClient client)
        {
            //await _readLock.WaitAsync();
            try
            {
                if (client == null || !client.Connected)
                {
                    throw new Exception("Client is disconnected.");
                }

                var headerMessage = new byte[HEADER_SIZE];
                int bytesRead = await client.GetStream().ReadAsync(headerMessage, 0, HEADER_SIZE);
                if (bytesRead < HEADER_SIZE) throw new Exception("Incomplete header received.");

                int messageSize = BitConverter.ToInt32(headerMessage, 1);
                var wholeMessage = new byte[messageSize];

                bytesRead = 0;
                while (bytesRead < messageSize)
                {
                    int read = await client.GetStream().ReadAsync(wholeMessage, bytesRead, messageSize - bytesRead);
                    if (read == 0) throw new Exception("Connection closed by server.");
                    bytesRead += read;
                }

                return new RequestInfo
                {
                    code = (RequestCode)headerMessage[0],
                    message = Encoding.UTF8.GetString(wholeMessage)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading message: {ex.Message}");
                return new RequestInfo { code = 0, message = "Failed to read message" };
            }
            finally
            {
                // _readLock.Release();
            }
        }

        public async Task HandleServerMessages()
        {
            while (true)
            {
                ResponseInfo response = await ReadMessageFromServer(_serverConnect);
                Console.WriteLine($"Received message from server: {response.code} - {response.message}");
            }
        }



    }
}
