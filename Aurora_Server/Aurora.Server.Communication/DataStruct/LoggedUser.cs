using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    public class LoggedUser
    {
        private string _username;
        private string _email;

        public LoggedUser(string username, string email)
        {
            _username = username;
            _email = email;
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
    }
}
