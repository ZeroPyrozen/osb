using System.Collections.Generic;
using System.Linq;

namespace osb.Models
{
    public class StoryboarderModel
    {
        public int UserID { get; private init; }
        public string Username { get; private init; }
        public string UserProfileUrl { get; private init; }
        public string UserAvatarUrl { get; private init; }
        public string UserCoverUrl { get; private init; }
        public CountryModel Country { get; private init; }

        private IEnumerable<DiscordRoleModel> Roles { get; }

        public StoryboarderModel(int userId, string username, string userCoverUrl,
            IEnumerable<DiscordRoleModel>? roles = default, CountryModel country = new())
        {
            UserID = userId;
            Username = username;
            UserProfileUrl = "https://osu.ppy.sh/users/" + userId;
            UserAvatarUrl = "https://a.ppy.sh/" + userId;
            UserCoverUrl = userCoverUrl;
            Roles = roles ?? Enumerable.Empty<DiscordRoleModel>();
            Country = country;
        }

        public DiscordRoleModel? GetPrimaryRole()
        {
            return Roles.MaxBy(x => x.RoleID);
        }
    }

    public class DiscordRoleModel
    {
        public static readonly DiscordRoleModel OSBWinner = new DiscordRoleModel(-2, "OSB Winner", "#ffe421");
        public static readonly DiscordRoleModel Sidetail = new DiscordRoleModel(-1, "Sidetail", "#206694");
        public static readonly DiscordRoleModel Verified = new DiscordRoleModel(0, "Verified", "#90a4b0");
        public static readonly DiscordRoleModel Apprentice = new DiscordRoleModel(1, "Apprentice", "#a79240");
        public static readonly DiscordRoleModel Storyboarder = new DiscordRoleModel(2, "Storyboarder", "#f1c40f");
        public static readonly DiscordRoleModel Contributor = new DiscordRoleModel(3, "Contributor", "#44acc0");
        public static readonly DiscordRoleModel Alumni = new DiscordRoleModel(4, "Alumni", "#9b59b6");
        public static readonly DiscordRoleModel Moderator = new DiscordRoleModel(5, "Moderator", "#3498db");
        public static readonly DiscordRoleModel Mentor = new DiscordRoleModel(6, "Mentor", "#2ecc71");

        public static readonly DiscordRoleModel OSBWebDeveloper =
            new DiscordRoleModel(int.MaxValue, "OSB WEB", "#44acc0");

        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string RoleHexColour { get; set; }

        public DiscordRoleModel(int RoleID, string RoleName, string RoleHexColour)
        {
            this.RoleID = RoleID;
            this.RoleName = RoleName;
            this.RoleHexColour = RoleHexColour;
        }
    }
}