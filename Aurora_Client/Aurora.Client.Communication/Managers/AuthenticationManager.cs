using System;
using System.Text.Json;
using System.Threading.Tasks;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using Newtonsoft.Json;

namespace Aurora.Client.Communication.Managers
{
    public class AuthenticationManager
    {
        private static AuthenticationManager _instance = null!;
        public static AuthenticationManager Instance
        {
            get
            {
                _instance ??= new AuthenticationManager();
                return _instance;
            }
        }

        public async Task<ResponseInfo> TrySigninginToServerWithToken()
        {
            ResponseInfo responseInfo = new ResponseInfo();
            RequestInfo requestInfo = new RequestInfo();

            if (!TokenManager.Instance.IsTokenExist())
            {
                responseInfo.code = ResponseCode.TOKEN_CONNECT_FAILED;
                responseInfo.message = "Cannot connect";
                return responseInfo;
            }

            requestInfo.message = await TokenManager.Instance.LoadTokenFromFile();
            requestInfo.code = RequestCode.CONNECT_REQUEST_CODE;
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, requestInfo);
            responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            return responseInfo;
        }

        public async Task<ResponseInfo> SigninToServer(string username, string password)
        {
            RequestInfo requestInfo = new RequestInfo
            {
                message = JsonConvert.SerializeObject(new { Username = username, Password = password }),
                code = RequestCode.LOGIN_REQUEST_CODE
            };

            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, requestInfo);
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);

            if (responseInfo.code == ResponseCode.TOKEN_LOGIN_SUCCESS)
            {
                await TokenManager.Instance.SaveTokenToFileAsync(responseInfo.message);
            }

            return responseInfo;
        }

        public async Task<ResponseInfo> SignupToServer(string username, string password, string email)
        {
            RequestInfo requestInfo = new RequestInfo
            {
                message = JsonConvert.SerializeObject(new { Username = username, Password = password, Email = email }),
                code = RequestCode.SIGN_UP_REQUEST_CODE
            };

            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, requestInfo);
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);

            if (responseInfo.code == ResponseCode.TOKEN_SIGNUP_SUCCESS)
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenConnectData>(responseInfo.message);
                await TokenManager.Instance.SaveTokenToFileAsync(data.Token);
            }

            return responseInfo;
        }
    }
}
