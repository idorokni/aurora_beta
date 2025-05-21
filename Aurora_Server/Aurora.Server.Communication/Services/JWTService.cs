using Aurora.Server.Communication.DataStruct;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Services
{
    public static class JWTService
    {
        private static readonly string _secretKey = "F90A311A6FEF39D337A088496969B9AC";
        private static readonly string _issuer = "localhost";
        private static readonly string _audience = "Aurora.Server";

        public static async Task<string> GenerateTokenAsync(string username, string password, string email)
        {
            return await Task.Run(() =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("username", username),
                        new Claim("email", email),
                        new Claim("password", password)  // Custom claim for password if necessary
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),  // Set token expiration
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            });
        }


        public static async Task<LoggedUser> ValidateTokenAsync(string token)
        {
            return await Task.Run(() =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    ValidateLifetime = true
                };

                try
                {
                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                    var jwtToken = validatedToken as JwtSecurityToken;
                    if (jwtToken != null)
                    {
                        var claimsList = jwtToken.Claims.ToList();
                        var username = claimsList.Find(claim => claim.Type == "username").Value;
                        var email = claimsList.Find(claim => claim.Type == "email").Value;
                        var user = new LoggedUser(username, email);
                        return user;

                    }
                    return null;  // Token is valid
                }
                catch (Exception ex)
                {
                    return null;  // Token is invalid
                }
            });
        }
    }
}
