using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Managers
{
    public class EncryptionManager
    {
        /// <summary>
        /// Encrypts the given plaintext using AES-CBC with PKCS7 padding.
        /// </summary>
        public static async Task<byte[]> Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (data == null || key == null || iv == null)
                throw new ArgumentNullException("Data, key, and IV must all be non-null.");

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            cryptoStream.FlushFinalBlock();
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts the given ciphertext using AES-CBC with PKCS7 padding.
        /// </summary>
        public static async Task<byte[]> Decrypt(byte[] encryptedData, byte[] key, byte[] iv)
        {
            if (encryptedData == null || key == null || iv == null)
                throw new ArgumentNullException("Encrypted data, key, and IV must all be non-null.");

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(encryptedData, 0, encryptedData.Length).ConfigureAwait(false);
            cryptoStream.FlushFinalBlock();
            return ms.ToArray();
        }
    }
}
