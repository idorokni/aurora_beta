using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Server.Communication;
using Aurora.Server.Database;
using Aurora.Server.Database.Data;

namespace Aurora.Server.Communication.Infrustructure
{
    public class Server
    {
        private static Server _instance;
        public static Server Instance
        {
            get
            {
                _instance ??= new Server();
                return _instance;
            }
        }

        public void RunServer()
        {
            Task.Run(() => { Communicator.Instance.AcceptClients(); });
            var code = Console.ReadLine();
            while (code is not "EXIT")
            {
                code = Console.ReadLine();
            }
        }
    }
}
