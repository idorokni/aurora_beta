using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData.Model
{
    public class DefultViewPostModel
    {
        public int UserID { get; set; }
        public int PostID { get; set; }
        public byte[] Image { get; set; } = null!;
    }
}
