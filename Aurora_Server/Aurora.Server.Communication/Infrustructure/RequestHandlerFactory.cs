using Aurora.Server.Communication.RequestHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Server.Communication.RequestHandlers;
using Aurora.Server.Communication.DataStruct;

namespace Aurora.Server.Communication.Infrustructure
{
    public class RequestHandlerFactory
    {
        private static RequestHandlerFactory _instance;

        public static RequestHandlerFactory Instance
        {
            get
            {
                _instance ??= new RequestHandlerFactory();
                return _instance;
            }
        }

        public JWTLoginRequestHandler GetJWTRequestHandler() => new JWTLoginRequestHandler();

        public HomeRequestHandler GetHomeRequestHandler(int userID) => new HomeRequestHandler(userID);
    }
}
