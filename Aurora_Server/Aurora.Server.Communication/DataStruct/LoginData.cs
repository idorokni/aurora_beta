using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    public class LoginData
    {
        private string _username;
        private string _password;

        public string Password { get { return _password; } set { _password = value; } }
        public string Username { get { return _username; } set { _username = value; } }
    }
}
