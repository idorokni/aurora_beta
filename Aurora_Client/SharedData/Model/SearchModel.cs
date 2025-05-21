using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData.Model
{
    public class SearchModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public int Followers { get; set; }
        public int Following { get; set; }
    }
}
