using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    public class ChatData
    {
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Message { get; set; } = null!;
    }
}
