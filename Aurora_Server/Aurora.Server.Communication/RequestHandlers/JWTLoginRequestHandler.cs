using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Aurora.Server.Database.Data;
using System.Net.Http.Headers;
using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Managers;
using Aurora.Server.Communication.Infrustructure;
using Aurora.Server.Communication.Codes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Aurora.Server.Communication.RequestHandlers
{
    public class JWTLoginRequestHandler : IRequestHandler
    {
        public bool CheckIfMatchRegex(string username, string password, string email = null)
        {
            bool isValid = true;

            isValid = Constants.PasswordRegex.IsMatch(password);
            if (email == null)
            {
                return isValid;
            }

            isValid = Constants.EmailRegex.IsMatch(email) && isValid;
            return isValid;
        }

        public bool IsRequestValid(RequestInfo info)
        {
            return info.code == RequestCode.LOGIN_REQUEST_CODE || info.code == RequestCode.SIGN_UP_REQUEST_CODE || info.code == RequestCode.CONNECT_REQUEST_CODE || info.code == RequestCode.PORT_SEND_REQUEST_CODE;
        }
        public async Task<(IRequestHandler, ResponseInfo)> HandleRequest(RequestInfo info)
        {
            ResponseInfo result = new ResponseInfo(); // Initialize `result`
            var userData = new UserData();
            IRequestHandler newRequestHandler =  RequestHandlerFactory.Instance.GetJWTRequestHandler();

            switch (info.code)
            {
                case RequestCode.LOGIN_REQUEST_CODE:
                    var loginData = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginData>(info.data);
                    if (loginData == null)
                    {
                        result.message = "error logging in";
                        result.code = ResponseCode.TOKEN_LOGIN_FAILED;  
                        break;
                    }

                    if (!CheckIfMatchRegex(loginData.Username, loginData.Password))
                    {
                        result.message = "Password not strong enough";
                        result.code = ResponseCode.TOKEN_LOGIN_FAILED;
                        break;
                    }

                    if (!await DatabaseManager.Instance.UserExists(loginData.Username) ||
                        !await DatabaseManager.Instance.checkIfPasswordsMatch(loginData.Username, loginData.Password))
                    {
                        result.message = "user and password don't match";
                        result.code = ResponseCode.TOKEN_LOGIN_FAILED;
                        break;
                    }

                    var loginResult = await JWTLoginManager.Instance.JWTLoginAsync(loginData.Username, loginData.Password);

                    result.message = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        Token = loginResult.Value.Item1,
                        Data = loginResult.Value.Item3,
                    });
                    result.code = ResponseCode.TOKEN_LOGIN_SUCCESS;
                    newRequestHandler = RequestHandlerFactory.Instance.GetHomeRequestHandler(loginResult.Value.Item3);
                    break;

                case RequestCode.SIGN_UP_REQUEST_CODE:
                    var signupData = Newtonsoft.Json.JsonConvert.DeserializeObject<SignupData>(info.data);
                    if (signupData == null)
                    {
                        result.message = "Invalid signup data";
                        result.code = ResponseCode.TOKEN_SIGNUP_FAILED;
                        break;
                    }

                    if (!CheckIfMatchRegex(signupData.Username, signupData.Password, signupData.Email))
                    {
                        result.message = "Password not strong enough or email is not in correct format";
                        result.code = ResponseCode.TOKEN_SIGNUP_FAILED;
                        break;
                    }

                    if (await DatabaseManager.Instance.UserExists(signupData.Username))
                    {
                        result.message = "user already exists";
                        result.code = ResponseCode.TOKEN_SIGNUP_FAILED;
                        break;
                    }
                    var addResult = await DatabaseManager.Instance.AddUser(signupData.Username, signupData.Password, signupData.Email);
                    result.message = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        Token = await JWTLoginManager.Instance.JWTSignupAsync(signupData.Username, signupData.Password, signupData.Email),
                        Data = addResult.Item3
                    });
                    result.code = ResponseCode.TOKEN_SIGNUP_SUCCESS;
                    newRequestHandler = RequestHandlerFactory.Instance.GetHomeRequestHandler(addResult.Item3);
                    break;

                case RequestCode.CONNECT_REQUEST_CODE:
                    string jwtToken = info.data;
                    var user = await JWTLoginManager.Instance.JWTConnectAsync(jwtToken);

                    if (user == null)
                    {
                        result.message = "cant connect";
                        result.code = ResponseCode.TOKEN_CONNECT_FAILED;
                        break;
                    }

                    var getResult = await DatabaseManager.Instance.GetUser(user.Username);
                    userData = getResult.Item1;

                    result.message = Newtonsoft.Json.JsonConvert.SerializeObject(getResult.Item3);
                    result.code = ResponseCode.TOKEN_CONNECT_SUCCESS;
                    newRequestHandler = RequestHandlerFactory.Instance.GetHomeRequestHandler(getResult.Item3);
                    break;

                case RequestCode.PORT_SEND_REQUEST_CODE:


                default:
                    result.message = "Invalid request";
                    result.code = ResponseCode.TOKEN_LOGIN_FAILED;
                    break;
            }

            return (newRequestHandler, result);
        }

    }
}