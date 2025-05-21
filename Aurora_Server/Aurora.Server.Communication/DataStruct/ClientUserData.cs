using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    public class ClientUserData
    {
        private string _username;
        private string _email;
        private string _bio;
        private string _joinDate;
        private string _birthday;
        private int _followers;
        private int _following;
        private byte[] _profilePicture;

        public string Birthday { get { return _birthday; } set { _birthday = value; } }
        public string Username { get { return _username; } set { _username = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Bio { get { return _bio; } set { _bio = value; } }
        public string JoinDate { get { return _joinDate; } set { _joinDate = value; } }
        public int Followers { get { return _followers; } set { _followers = value; } }
        public int Following { get { return _following; } set { _following = value; } }
        public byte[] ProfilePicture { get { return _profilePicture; } set { _profilePicture = value; } }
    }
}
