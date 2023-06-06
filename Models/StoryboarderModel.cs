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
            IEnumerable<DiscordRoleModel>? roles = default, CountryModel country = default)
        {
            UserID = userId;
            Username = username;
            UserProfileUrl = "https://osu.ppy.sh/users/" + userId;
            UserAvatarUrl = "https://a.ppy.sh/" + userId;
            UserCoverUrl = userCoverUrl;
            Roles = roles ?? Enumerable.Empty<DiscordRoleModel>();
            Country = country == default ? CountryModel.Unknown : country;
        }

        public DiscordRoleModel? GetPrimaryRole()
        {
            return Roles.MaxBy(x => x.RoleID);
        }
    }
}