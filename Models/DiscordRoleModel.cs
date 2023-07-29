namespace osb.Models;

public class DiscordRoleModel
{
    public static readonly DiscordRoleModel OSBWinner = new DiscordRoleModel(-2, "OSB Winner", "#ffe421");
    public static readonly DiscordRoleModel Sidetail = new DiscordRoleModel(-1, "Sidetail", "#206694");
    public static readonly DiscordRoleModel Verified = new DiscordRoleModel(0, "Verified", "#90a4b0");
    public static readonly DiscordRoleModel Apprentice = new DiscordRoleModel(1, "Apprentice", "#C89152");
    public static readonly DiscordRoleModel Storyboarder = new DiscordRoleModel(2, "Storyboarder", "#FBE363");
    public static readonly DiscordRoleModel Contributor = new DiscordRoleModel(3, "Contributor", "#47DFBB");
    public static readonly DiscordRoleModel Alumni = new DiscordRoleModel(4, "Alumni", "#B856F3");
    public static readonly DiscordRoleModel Moderator = new DiscordRoleModel(5, "Moderator", "#64BEFF");
    public static readonly DiscordRoleModel Mentor = new DiscordRoleModel(6, "Mentor", "#87F560");

    public static readonly DiscordRoleModel OSBWebDeveloper =
        new DiscordRoleModel(7, "Osb Web", "#64E9BF");
    public static readonly DiscordRoleModel Designer =
        new DiscordRoleModel(int.MaxValue, "Designer", "#00FF85");

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