using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Managers
{
    public class TokenManager
    {
        private static TokenManager _instance;
        public static TokenManager Instance
        {
            get
            {
                _instance ??= new TokenManager();
                return _instance;
            }
        }
        private static readonly string tokenFilePath = "token.dat";

        public async Task SaveTokenToFileAsync(string token)
        {
            await File.WriteAllTextAsync(tokenFilePath, token);
        }

        public async Task<string> LoadTokenFromFile()
        {
            try
            {
                if (!IsTokenExist())
                    return string.Empty; // Return an empty string if the token file does not exist.

                var token = await File.ReadAllTextAsync(tokenFilePath);
                return token;
            }
            catch (Exception ex)
            {
                // Log or handle the exception here
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }


        public bool IsTokenExist()
        {
            return File.Exists(tokenFilePath);
        }
    }
}
