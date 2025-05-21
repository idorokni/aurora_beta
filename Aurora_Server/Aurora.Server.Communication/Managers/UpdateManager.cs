using Aurora.Server.Communication.Codes;
using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Infrustructure;
using Aurora.Server.Communication.RequestHandlers;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Managers
{
    public class UpdateManager
    {
        private static UpdateManager _instance;
        public static UpdateManager Instance
        {
            get
            {
                _instance ??= new UpdateManager();
                return _instance;
            }
        }

        public async Task<Dictionary<TcpClient, RequestInfo>> ManageClientUpdate(RequestCode code, object data, ResponseInfo result)
        {
            var returnDict = new Dictionary<TcpClient, RequestInfo>();
            switch (code)
            {
                case RequestCode.SEND_MESSAGE_REQUEST_CODE:
                    returnDict = await HandleSendMessage(data as ChatData);
                    break;
                case RequestCode.LOGIN_REQUEST_CODE:
                    returnDict = await HandleLogin(data as LoginData);
                    break;
                case RequestCode.SIGN_UP_REQUEST_CODE:
                    returnDict = await HandleSignup(data as SignupData);
                    break;
                case RequestCode.CONNECT_REQUEST_CODE:
                    returnDict = await HandleToken(data as string);
                    break;
                default:
                    break;
            }

            return returnDict;
        }

        private async Task<Dictionary<TcpClient, RequestInfo>> HandleSendMessage(ChatData messageData)
        {
            var returnDict = new Dictionary<TcpClient, RequestInfo>();
            var request = new RequestInfo
            {
                code = RequestCode.SEND_MESSAGE_REQUEST_CODE,
                data = Newtonsoft.Json.JsonConvert.SerializeObject(messageData)
            };

            var recipient = Communicator.Instance.Clients
                .Where(c => c.Value is HomeRequestHandler handler && handler.UserID == messageData.ReceiverID)
                .FirstOrDefault().Key;

            returnDict[recipient] = request;

            return returnDict;
        }

        private async Task<Dictionary<TcpClient, RequestInfo>> HandleLogin(LoginData loginData)
        {
            var returnDict = new Dictionary<TcpClient, RequestInfo>();
            var userData = await DatabaseManager.Instance.GetUser(loginData.Username);

            var request = new RequestInfo
            {
                code = RequestCode.LOGIN_REQUEST_CODE,
                data = Newtonsoft.Json.JsonConvert.SerializeObject(userData)
            };


            var users = await DatabaseManager.Instance.GetFollowedUsers(userData.Item3);
            Communicator.Instance.Clients.Where(c => c.Value is HomeRequestHandler handler && users.Select(t => t.Item1).Contains(handler.UserID))
                .Select(c => c.Key)
                .ToList()
                .ForEach(client =>
                {
                    returnDict[client] = request;
                });
            return returnDict;
        }

        private async Task<Dictionary<TcpClient, RequestInfo>> HandleSignup(SignupData signupData)
        {
            var returnDict = new Dictionary<TcpClient, RequestInfo>();
            var userData = await DatabaseManager.Instance.GetUser(signupData.Username);

            var request = new RequestInfo
            {
                code = RequestCode.SIGN_UP_REQUEST_CODE,
                data = Newtonsoft.Json.JsonConvert.SerializeObject(userData)
            };

            var users = await DatabaseManager.Instance.GetFollowedUsers(userData.Item3);
            Communicator.Instance.Clients.Where(c => c.Value is HomeRequestHandler handler && users.Select(t => t.Item1).Contains(handler.UserID))
                .Select(c => c.Key)
                .ToList()
                .ForEach(client =>
                {
                    returnDict[client] = request;
                });
            return returnDict;
        }

        private async Task<Dictionary<TcpClient, RequestInfo>> HandleToken(string token)
        {
            var returnDict = new Dictionary<TcpClient, RequestInfo>();
            var userData = await DatabaseManager.Instance.GetUser((await JWTLoginManager.Instance.JWTConnectAsync(token)).Username);

            var request = new RequestInfo
            {
                code = RequestCode.CONNECT_REQUEST_CODE,
                data = Newtonsoft.Json.JsonConvert.SerializeObject(userData)
            };

            var users = await DatabaseManager.Instance.GetFollowedUsers(userData.Item3);
            Communicator.Instance.Clients.Where(c => c.Value is HomeRequestHandler handler && users.Select(t => t.Item1).Contains(handler.UserID))
                .Select(c => c.Key)
                .ToList()
                .ForEach(client =>
                {
                    returnDict[client] = request;
                });
            return returnDict;
        }

        /*
        private async Task<Dictionary<int, RequestInfo>> HandleClientUpdateOnlineUsers(UserData data)
        {
            var returnDict = new Dictionary<int, RequestInfo>();
            var request = new RequestInfo
            {
                code = RequestCode.GET_ONLINE_USERS_REQUEST_CODE,
                data = Newtonsoft.Json.JsonConvert.SerializeObject(data)
            };
            Communicator.Instance.Clients
                .Where(x => x.Value is HomeRequestHandler)
                .Select(x => (x.Value as HomeRequestHandler).UserID)
                .ToList()
                .ForEach(id =>
                {
                    returnDict[id] = request;
                });

            return returnDict;

        }
        */


    }
}
