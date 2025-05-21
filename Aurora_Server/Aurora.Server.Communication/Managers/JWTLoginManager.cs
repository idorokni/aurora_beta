using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Services;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Managers
{
    public class JWTLoginManager
    {
        private static JWTLoginManager _instance;
        public static JWTLoginManager Instance
        {
            get
            {
                _instance ??= new JWTLoginManager();
                return _instance;
            }
        }
        public async Task<string> JWTSignupAsync(string username, string password, string email)
        {
            try
            {
                return await JWTService.GenerateTokenAsync(username, password, email);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(string, UserData, int)?> JWTLoginAsync(string username, string password)
        {
            try
            {
                var result = await DatabaseManager.Instance.GetUser(username);
                return (await JWTService.GenerateTokenAsync(username, password, result.Item1.Email), result.Item1, result.Item3);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<LoggedUser> JWTConnectAsync(string token)
        {
            return await JWTService.ValidateTokenAsync(token);
        }
    }
}
