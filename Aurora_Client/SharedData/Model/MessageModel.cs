using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData.Model
{
    public class MessageModel
    {
        public string Content { get; set; } = null!;
        public bool IsSender { get; set; }
    }
}
