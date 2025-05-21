using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Database.Models
{
    public class Follow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RelationID { get; set; }

        public int FollowerUserID { get; set; }

        public int FollowedUserID { get; set; }
    }
}
