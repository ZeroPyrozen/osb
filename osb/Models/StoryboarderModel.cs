﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace osb.Models
{
    public class StoryboarderModel
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string UserProfileUrl { get; set; }
        public string UserAvatarUrl { get; set; }
        public List<DiscordRoleModel> Roles { get; set; }

        public StoryboarderModel(string UserID, string Username, List<DiscordRoleModel> Roles)
        {
            this.UserID = UserID;
            //TODO: I will create get username more dynamic to update with name change
            this.Username = Username;
            this.Roles = Roles;
            this.UserAvatarUrl = "https://a.ppy.sh/" + UserID;
            this.UserProfileUrl = "https://osu.ppy.sh/users/" + UserID;
        }
    }

    public class DiscordRoleModel
    {
        public static readonly DiscordRoleModel Sidetail = new DiscordRoleModel(-1, "Sidetail", "#206694");
        public static readonly DiscordRoleModel Verified = new DiscordRoleModel(0, "Verified", "#90a4b0");
        public static readonly DiscordRoleModel Apprentice = new DiscordRoleModel(1, "Apprentice", "#a79240");
        public static readonly DiscordRoleModel Storyboarder = new DiscordRoleModel(2, "Storyboarder", "#f1c40f");
        public static readonly DiscordRoleModel Alumni = new DiscordRoleModel(3, "Alumni", "#9b59b6");
        public static readonly DiscordRoleModel Moderator = new DiscordRoleModel(3, "Moderator", "#3498db");
        public static readonly DiscordRoleModel Mentor = new DiscordRoleModel(3, "Mentor", "#2ecc71");

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
