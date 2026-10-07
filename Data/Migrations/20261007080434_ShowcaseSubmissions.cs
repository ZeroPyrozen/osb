using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace osb.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShowcaseSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedOnSiteAt",
                table: "Beatmapsets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShowcaseRemovals",
                columns: table => new
                {
                    BeatmapsetId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RemovedById = table.Column<int>(type: "INTEGER", nullable: false),
                    RemovedByUsername = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseRemovals", x => x.BeatmapsetId);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeatmapsetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Artist = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    HostId = table.Column<int>(type: "INTEGER", nullable: false),
                    HostUsername = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    BeatmapSubmittedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    OsuListsStoryboard = table.Column<bool>(type: "INTEGER", nullable: false),
                    Medium = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SubmitterId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmitterUsername = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReviewerUsername = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    OutcomeSeen = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionCredits",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OsuUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionCredits", x => new { x.SubmissionId, x.OsuUserId });
                    table.ForeignKey(
                        name: "FK_SubmissionCredits_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionTags",
                columns: table => new
                {
                    ShowcaseSubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SuggestedTagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionTags", x => new { x.ShowcaseSubmissionId, x.SuggestedTagsId });
                    table.ForeignKey(
                        name: "FK_SubmissionTags_Submissions_ShowcaseSubmissionId",
                        column: x => x.ShowcaseSubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionTags_Tags_SuggestedTagsId",
                        column: x => x.SuggestedTagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_BeatmapsetId",
                table: "Submissions",
                column: "BeatmapsetId",
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_Status",
                table: "Submissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmitterId",
                table: "Submissions",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTags_SuggestedTagsId",
                table: "SubmissionTags",
                column: "SuggestedTagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowcaseRemovals");

            migrationBuilder.DropTable(
                name: "SubmissionCredits");

            migrationBuilder.DropTable(
                name: "SubmissionTags");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropColumn(
                name: "ChangedOnSiteAt",
                table: "Beatmapsets");
        }
    }
}
