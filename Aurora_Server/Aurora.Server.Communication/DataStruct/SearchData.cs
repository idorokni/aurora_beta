using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    internal class SearchData
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
    }
}
