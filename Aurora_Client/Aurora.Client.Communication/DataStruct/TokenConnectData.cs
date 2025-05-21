using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.DataStruct
{
    public class TokenConnectData
    {
        public string Token { get; set; }
        public int Data { get; set; }
    }
}
