using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Server.Communication.Infrustructure;

namespace Aurora.Server.Communication
{
    public class BootServer
    {
        static void Main(string[] args) => Infrustructure.Server.Instance.RunServer();
    }
}
