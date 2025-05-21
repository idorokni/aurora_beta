using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.DataStruct
{
    public class LoginReturnData
    {
        private string _token;
        private string _email;

        public string Email { get { return _email; } set { _email = value; } }
        public string Token { get { return _token; } set { _token = value; } }
    }
}
