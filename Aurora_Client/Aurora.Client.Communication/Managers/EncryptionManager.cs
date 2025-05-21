using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Managers
{
    public class EncryptionManager
    {
        private static EncryptionManager _instance = null!;
        public static EncryptionManager Instance
        {
            get
            {
                _instance ??= new EncryptionManager();
                return _instance;
            }
        }

        public byte[] AesKey { get; set; } = null!;
        public byte[] AesIV { get; set; } = null!;

        public async Task<byte[]> Encrypt(RequestInfo request)
        {
            if (AesKey == null || AesIV == null)
                throw new InvalidOperationException("AES key and IV must be set before encryption.");

            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIV;

            var plainBytes = Encoding.UTF8.GetBytes(request.message);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                await cryptoStream.WriteAsync(plainBytes, 0, plainBytes.Length);
            }

            return ms.ToArray();
        }

        public async Task<string> Decrypt(byte[] response)
        {
            if (AesKey == null || AesIV == null)
                throw new InvalidOperationException("AES key and IV must be set before decryption.");

            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIV;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(response);
            using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cryptoStream, Encoding.UTF8);

            var json = await sr.ReadToEndAsync();
            return json;
        }

        public async Task<bool> SetAesEncryption(string rsaPublicKey)
        {
            using (Aes aes = Aes.Create())
            {
                // Generate fresh AES parameters
                aes.GenerateKey();
                aes.GenerateIV();
                AesKey = aes.Key;
                AesIV = aes.IV;

                // Create and serialize AES parameters
                var aesParams = new
                {
                    Key = Convert.ToBase64String(AesKey),
                    IV = Convert.ToBase64String(AesIV)
                };
                string jsonAesParams = JsonConvert.SerializeObject(aesParams);

                // Encrypt with RSA public key
                byte[] encryptedParams;
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromPem(rsaPublicKey);
                    encryptedParams = rsa.Encrypt(
                        Encoding.UTF8.GetBytes(jsonAesParams),
                        RSAEncryptionPadding.OaepSHA256
                    );
                }

                // Create and send request
                var request = new RequestInfo
                {
                    code = RequestCode.SEND_AES_SETUP_REQUEST_CODE,
                    message = Convert.ToBase64String(encryptedParams)
                };

                await Communicator.Instance.SendMessageToServer(
                    Communicator.Instance.Client,
                    request
                );

                // Wait for server confirmation
                var response = await Communicator.Instance.ReadMessageFromServer(
                    Communicator.Instance.Client
                );

                return response.code == ResponseCode.SEND_AES_SETUP_SUCCESS;
            }
        }
    }
}
