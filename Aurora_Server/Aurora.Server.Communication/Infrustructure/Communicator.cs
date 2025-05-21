using Aurora.Server.Communication.Codes;
using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Managers;
using Aurora.Server.Communication.RequestHandlers;
using Aurora.Server.Communication.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Aurora.Server.Communication.Infrustructure
{
    public class Communicator
    {
        private static Communicator _instance;
        private readonly Dictionary<TcpClient, IRequestHandler> _clients;
        private readonly ConcurrentDictionary<TcpClient, SemaphoreSlim> _clientLocks;
        private readonly Dictionary<TcpClient, TcpClient> _listeners;
        private readonly Dictionary<TcpClient, RequestInfo> _lastRequest;
        private readonly TcpListener _server;

        private const int SERVER_LISTEN_PORT = 1223;
        private const int BUFFER_SIZE = 1024;
        private const int CODE_AMOUNT_BYTES = 1;
        private const int BYTES_LENGTH = 4;
        private const int HEADER_SIZE = CODE_AMOUNT_BYTES + BYTES_LENGTH;

        public Dictionary<TcpClient, IRequestHandler> Clients => _clients;


        private Communicator()
        {
            _clients = new Dictionary<TcpClient, IRequestHandler>();
            _clientLocks = new ConcurrentDictionary<TcpClient, SemaphoreSlim>();
            _server = new TcpListener(IPAddress.Any, SERVER_LISTEN_PORT);
            _listeners = new Dictionary<TcpClient, TcpClient>();
            _lastRequest = new Dictionary<TcpClient, RequestInfo>();
            _server.Start();
        }

        public static Communicator Instance => _instance ??= new Communicator();

        public void AcceptClients()
        {
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                _clients[client] = RequestHandlerFactory.Instance.GetJWTRequestHandler();
                _clientLocks[client] = new SemaphoreSlim(1, 1); // Per-client lock
                _ = Task.Run(() => HandleClientAsync(client, _clients[client]));
            }
        }

        private async Task<RequestInfo> ReadMessage(NetworkStream stream)
        {
            var header = new byte[HEADER_SIZE];
            int bytesRead = 0;

            // Read the entire header
            while (bytesRead < HEADER_SIZE)
            {
                int read = await stream.ReadAsync(header, bytesRead, HEADER_SIZE - bytesRead);
                if (read == 0) throw new Exception("Client disconnected.");
                bytesRead += read;
            }

            var length = BitConverter.ToInt32(header, CODE_AMOUNT_BYTES);
            var wholeMessage = new byte[length];
            bytesRead = 0;

            // Read the entire message body
            while (bytesRead < length)
            {
                int read = await stream.ReadAsync(wholeMessage, bytesRead, length - bytesRead);
                if (read == 0) throw new Exception("Client disconnected.");
                bytesRead += read;
            }

            return new RequestInfo
            {
                code = (RequestCode)header[0],
                data = Encoding.UTF8.GetString(wholeMessage).Trim('\0')
            };
        }

        private async Task<RequestInfo> ReadMessageEncrypted(NetworkStream stream, byte[] iv, byte[] key)
        {
            var header = new byte[HEADER_SIZE];
            int bytesRead = 0;

            // Read the entire header
            while (bytesRead < HEADER_SIZE)
            {
                int read = await stream.ReadAsync(header, bytesRead, HEADER_SIZE - bytesRead);
                if (read == 0) throw new Exception("Client disconnected.");
                bytesRead += read;
            }

            var length = BitConverter.ToInt32(header, CODE_AMOUNT_BYTES);
            var wholeMessage = new byte[length];
            bytesRead = 0;

            // Read the entire message body
            while (bytesRead < length)
            {
                int read = await stream.ReadAsync(wholeMessage, bytesRead, length - bytesRead);
                if (read == 0) throw new Exception("Client disconnected.");
                bytesRead += read;
            }

            return new RequestInfo
            {
                code = (RequestCode)header[0],
                data = Encoding.UTF8.GetString(await EncryptionManager.Decrypt(wholeMessage, key, iv))
            };
        }


        private async Task SendMessage(TcpClient client, ResponseInfo result)
        {
            //if (!_clientLocks.TryGetValue(client, out var clientLock))
                //return; // Client might have disconnected

            //await clientLock.WaitAsync(); // Lock per client
            try
            {
                var wholeMessage = new byte[HEADER_SIZE + result.message.Length];
                wholeMessage[0] = (byte)result.code;
                byte[] arr = BitConverter.GetBytes(result.message.Length);
                Array.Copy(arr, 0, wholeMessage, 1, 4);
                Array.Copy(Encoding.UTF8.GetBytes(result.message), 0, wholeMessage, HEADER_SIZE, result.message.Length);
                await client.GetStream().WriteAsync(wholeMessage);
            }
            finally
            {
                //clientLock.Release();
            }
        }

        private async Task SendMessageEncrypted(TcpClient client, ResponseInfo result, byte[] iv, byte[] key)
        {
            //if (!_clientLocks.TryGetValue(client, out var clientLock))
            //return; // Client might have disconnected

            //await clientLock.WaitAsync(); // Lock per client
            try
            {
                var encryptedMessage = await EncryptionManager.Encrypt(Encoding.UTF8.GetBytes(result.message), key, iv);
                var wholeMessage = new byte[HEADER_SIZE + encryptedMessage.Length];
                wholeMessage[0] = (byte)result.code;
                byte[] arr = BitConverter.GetBytes(encryptedMessage.Length);
                Array.Copy(arr, 0, wholeMessage, 1, 4);
                Array.Copy(encryptedMessage, 0, wholeMessage, HEADER_SIZE, encryptedMessage.Length);
                await client.GetStream().WriteAsync(wholeMessage);
            }
            finally
            {
                //clientLock.Release();
            }
        }

        private async Task SendMessageEncrypted(TcpClient client, RequestInfo result, byte[] iv, byte[] key)
        {
            //if (!_clientLocks.TryGetValue(client, out var clientLock))
            //return; // Client might have disconnected

            //await clientLock.WaitAsync(); // Lock per client
            try
            {
                var encryptedMessage = await EncryptionManager.Encrypt(Encoding.UTF8.GetBytes(result.data), key, iv);
                var wholeMessage = new byte[HEADER_SIZE + encryptedMessage.Length];
                wholeMessage[0] = (byte)result.code;
                byte[] arr = BitConverter.GetBytes(encryptedMessage.Length);
                Array.Copy(arr, 0, wholeMessage, 1, 4);
                Array.Copy(encryptedMessage, 0, wholeMessage, HEADER_SIZE, encryptedMessage.Length);
                await client.GetStream().WriteAsync(wholeMessage);
            }
            finally
            {
                //clientLock.Release();
            }
        }

        private async Task SendMessage(TcpClient client, RequestInfo result)
        {
            //if (!_clientLocks.TryGetValue(client, out var clientLock))
            //return; // Client might have disconnected

            //await clientLock.WaitAsync(); // Lock per client
            try
            {
                var wholeMessage = new byte[HEADER_SIZE + result.data.Length];
                wholeMessage[0] = (byte)result.code;
                byte[] arr = BitConverter.GetBytes(result.data.Length);
                Array.Copy(arr, 0, wholeMessage, 1, 4);
                Array.Copy(Encoding.UTF8.GetBytes(result.data), 0, wholeMessage, HEADER_SIZE, result.data.Length);
                await client.GetStream().WriteAsync(wholeMessage);
            }
            finally
            {
                //clientLock.Release();
            }
        }

        private async Task HandleClientAsync(TcpClient client, IRequestHandler handler)
        {
            byte[] aesKey = null!;
            byte[] aesIV = null!;

            try
            {   
                RequestInfo info = await ReadMessage(client.GetStream());
                if (info.code != RequestCode.PORT_SEND_REQUEST_CODE)
                    return;

                var port = int.Parse(info.data);
                TcpClient tcpClient = new TcpClient();
                var num = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                tcpClient.Connect(num, port);
                _listeners[client] = tcpClient;

                await SendMessage(client, new ResponseInfo
                {
                    code = ResponseCode.PORT_SEND_SUCCESS,
                    message = "Connected to the server"
                });
                info = await ReadMessage(client.GetStream());
                var rsa = RSA.Create(2048);
                if (info.code != RequestCode.GET_SERVER_RSA_PUBLIC_KEY_REQUEST_CODE)
                    return;

                var result = new ResponseInfo
                {
                    code = ResponseCode.GET_SERVER_RSA_PUBLIC_KEY_REQUEST_CODE_SUCCESS,
                    message = ExportPublicKeyToPem(rsa)
                };
                await SendMessage(client, result);

                info = await ReadMessage(client.GetStream());
                if(info.code != RequestCode.SEND_AES_SETUP_REQUEST_CODE)
                    return;

                var aesData = Newtonsoft.Json.JsonConvert.DeserializeObject<AesExchangeData>(info.data);
                aesKey = rsa.Decrypt(aesData.key, RSAEncryptionPadding.Pkcs1);
                aesIV = rsa.Decrypt(aesData.iv, RSAEncryptionPadding.Pkcs1);
                result = new ResponseInfo
                {
                    code = ResponseCode.SEND_AES_SETUP_SUCCESS,
                    message = "AES setup successful"
                };
                await SendMessage(client, result);


                while (true)
                { 
                    info = await ReadMessageEncrypted(client.GetStream(), aesIV, aesKey);
                    if (handler.IsRequestValid(info))
                    {
                        _lastRequest[client] = info;
                        (handler, result) = await handler.HandleRequest(info);
                        _clients[client] = handler;
                        await SendMessageEncrypted(client, result, aesIV, aesKey); // Send response using per-client lock
                        await HandleClientUpdate(client, info, result, aesIV, aesKey);
                    }
                    else
                    {
                        throw new Exception("Invalid request");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
                _clients.Remove(client);
                _clientLocks.TryRemove(client, out _); // Remove client lock
                client.Close();
            }
        }

        
        private async Task HandleClientUpdate(TcpClient client, RequestInfo info, ResponseInfo result, byte[] iv, byte[] key)
        {
            try
            {
                (await UpdateManager.Instance.ManageClientUpdate(info.code, UpdateService.ParseData(info.code, info.data) ?? throw new Exception("Failed to parse data"), result))
                    .ToList()
                    .ForEach(async p =>
                    {
                        await SendMessageEncrypted(_listeners[p.Key], p.Value, iv, key);
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"not relevant");
                return;
            }
        }

        private string ExportPublicKeyToPem(RSA rsa)
        {
            var pem = new StringBuilder();
            pem.AppendLine("-----BEGIN PUBLIC KEY-----");
            pem.AppendLine(Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo(), Base64FormattingOptions.InsertLineBreaks));
            pem.AppendLine("-----END PUBLIC KEY-----");
            return pem.ToString();
        }



        /*
        private Tuple<List<TcpClient>, ResponseInfo> GetRelevantClients(RequestInfo info)
        {
            switch (code)
            {
                case RequestCode.SIGN_UP_REQUEST_CODE:
                    return new RequestCode[]
                    {
                        RequestCode.SEARCH_USER_REQUEST_CODE
                    };
                case RequestCode.ADD_POST_REQUEST_CODE:
                    return new RequestCode[]
                    {
                        RequestCode.GET_AMOUNT_OF_POSTS_REQUEST_CODE,
                        RequestCode.GET_POST_REQUEST_CODE
                    };
                case RequestCode.UPDATE_USER_DATA_REQUEST_CODE:
                    return new RequestCode[]
                    {
                        RequestCode.GET_USER_DATA_REQUEST_CODE,
                        RequestCode.SEARCH_USER_REQUEST_CODE,
                        RequestCode.UPDATE_USER_DATA_REQUEST_CODE
                    };
                case RequestCode.LIKE_POST_REQUEST_CODE:
                    return new RequestCode[]
                    {
                        RequestCode.LIKE_POST_REQUEST_CODE,
                        RequestCode.COMMENT_REQUEST_CODE,
                        RequestCode.GET_POST_DATA_REQUEST_CODE
                    };
                case RequestCode.COMMENT_REQUEST_CODE:
                    return new RequestCode[]
                    {
                        RequestCode.COMMENT_REQUEST_CODE,
                        RequestCode.LIKE_POST_REQUEST_CODE,
                        RequestCode.GET_POST_DATA_REQUEST_CODE
                    };
                default:
                    return null
            }


        }
        */

    }
}