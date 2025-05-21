using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData.Model
{
    public class UserChatModel
    {
        public int UserID { get; set; }
        public string Username { get; set; } = null!;
        public byte[] Image { get; set; } = null!;
    }
}
