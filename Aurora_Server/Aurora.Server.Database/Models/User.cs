using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Database.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string ProfileImagePath { get; set; } = null!;

        public string Bio { get; set; } = null!;

        public int Followers { get; set; }

        public int Following { get; set; }

        public string JoinDate { get; set; } = null!;

        public string Birthday { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    }
}
