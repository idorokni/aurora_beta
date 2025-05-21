using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.DataStruct
{
    public class UserData
    {
        public string Birthday { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string JoinDate { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public string ProfilePicture { get; set; }

    }
}
