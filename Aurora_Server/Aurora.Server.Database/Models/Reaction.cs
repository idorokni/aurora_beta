using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Database.Models
{
    public enum ReactionType : int
    {
        Like = 1,
        Dislike,
        SuperLike,
    }

    public class Reaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReactionID { get; set; }

        public int PostID { get; set; }

        public int UserID { get; set; }

        public ReactionType Type { get; set; }

        public virtual User UserReaction { get; set; } = null!;

        public virtual Post PostReaction { get; set; } = null!;

    }
}
