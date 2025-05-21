using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.DataStruct
{
    public class PostData
    {
        public string Description { get; set; } = null!;
        public int AmountOfLikes { get; set; }
        public int AmountOfDislikes { get; set; }
        public int AmountOfSuperLikes { get; set; }
        public List<CommentData> Comments { get; set; } = null!;
        public bool AlreadyLiked { get; set; }
        public bool AlreadyDisliked { get; set; }
        public bool AlreadySuperLiked { get; set; }

    }
}
