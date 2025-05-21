using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData.Model
{
    public class CommentModel
    {
        public string CommentContent { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public byte[] ProfilePicture { get; set; } = null!;
    }
}
