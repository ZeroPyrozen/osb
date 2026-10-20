using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace osb.Data.Migrations
{
    /// <summary>
    /// Fixes data; the schema doesn't change. Seven storyboards from the osb.moe archive were dated
    /// before osu! says their beatmapsets were submitted. showcase.json now gives them the submission
    /// date, and this does the same for databases that imported the old dates. Storyboards changed on
    /// the site are left alone, as the start-up import leaves them.
    /// </summary>
    public partial class FixEarlyShowcaseDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Beatmapsets" SET "ShowcasedOn" = "SubmittedOn"
                WHERE "ShowcasedOn" < "SubmittedOn" AND "ChangedOnSiteAt" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The old dates were mistakes, so there's nothing to put back.
        }
    }
}
