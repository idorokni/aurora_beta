using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.DataStruct
{
    public class AesExchangeData
    {
        public byte[] key { get; set; } = null!;
        public byte[] iv { get; set; } = null!;
    }
}
