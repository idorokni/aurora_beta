using Aurora.Server.Communication.Codes;
using Aurora.Server.Communication.DataStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Services
{
    public class UpdateService
    {
        public static object ParseData(RequestCode code, string data)
        {
            switch (code)
            {
                case RequestCode.SEND_MESSAGE_REQUEST_CODE:
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<ChatData>(data);
                case RequestCode.LOGIN_REQUEST_CODE:
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<LoginData>(data);
                case RequestCode.SIGN_UP_REQUEST_CODE:
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<SignupData>(data);
                case RequestCode.CONNECT_REQUEST_CODE:
                    return data;
                default:
                    return null;
            }
        }
    }
}
