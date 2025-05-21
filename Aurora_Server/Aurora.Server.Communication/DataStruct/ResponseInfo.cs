using Aurora.Server.Communication.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.DataStruct
{
    public struct ResponseInfo
    {
        public ResponseCode code;
        public string message;
    }
}
