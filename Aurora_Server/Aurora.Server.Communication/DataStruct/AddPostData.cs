using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    public class AddPostData
    {
        public string Description { get; set; }
        public byte[] ImageData { get; set; }
    }
}
