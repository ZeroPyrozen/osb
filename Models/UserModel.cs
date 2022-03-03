using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace osb.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string UserProfileUrl { get; set; }
        public string UserAvatarUrl { get; set; }

        public UserModel(int UserID, string Username)
        {
            this.UserID = UserID;
            this.Username = Username;
            this.UserAvatarUrl = "https://a.ppy.sh/" + UserID;
            this.UserProfileUrl = "https://osu.ppy.sh/users/" + UserID;
        }
    }
}
