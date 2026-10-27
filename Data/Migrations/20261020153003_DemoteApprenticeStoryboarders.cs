using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace osb.Data.Migrations
{
    /// <summary>
    /// Fixes data; the schema doesn't change. MRL (8309500) and Ponilek (10579063) were seeded as
    /// Storyboarders, but they are Apprentices on the osb Discord. showcase.json now gives them
    /// Apprentice, which the start-up import adds; it never removes a role, so this takes away the
    /// Storyboarder role in databases that imported the old one.
    /// </summary>
    public partial class DemoteApprenticeStoryboarders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "OsuUserRoles"
                WHERE "MembersId" IN (8309500, 10579063)
                  AND "RolesId" IN (SELECT "Id" FROM "Roles" WHERE "Name" = 'Storyboarder');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The old role was a mistake, so there's nothing to put back.
        }
    }
}
