using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Database.Models
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        public int UserID { get; set; } 

        public string Description { get; set; } = null!;

        public string PostPath { get; set; } = null!;

        public int AmountLikes { get; set; }

        public int AmountDislikes { get; set; }

        public int AmountSuperLikes { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
