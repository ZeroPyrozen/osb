using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace osb.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Learners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Colour = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 48, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 48, nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    IsCommunityMember = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitCompletions_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beatmapsets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Artist = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    HostId = table.Column<int>(type: "INTEGER", nullable: false),
                    Medium = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SubmittedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ShowcasedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beatmapsets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beatmapsets_Users_HostId",
                        column: x => x.HostId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OsuUserRoles",
                columns: table => new
                {
                    MembersId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OsuUserRoles", x => new { x.MembersId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_OsuUserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OsuUserRoles_Users_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapsetStoryboarders",
                columns: table => new
                {
                    BeatmapsetId = table.Column<int>(type: "INTEGER", nullable: false),
                    OsuUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapsetStoryboarders", x => new { x.BeatmapsetId, x.OsuUserId });
                    table.ForeignKey(
                        name: "FK_BeatmapsetStoryboarders_Beatmapsets_BeatmapsetId",
                        column: x => x.BeatmapsetId,
                        principalTable: "Beatmapsets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapsetStoryboarders_Users_OsuUserId",
                        column: x => x.OsuUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapsetTags",
                columns: table => new
                {
                    BeatmapsetsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapsetTags", x => new { x.BeatmapsetsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_BeatmapsetTags_Beatmapsets_BeatmapsetsId",
                        column: x => x.BeatmapsetsId,
                        principalTable: "Beatmapsets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapsetTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beatmapsets_HostId",
                table: "Beatmapsets",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_Beatmapsets_ShowcasedOn",
                table: "Beatmapsets",
                column: "ShowcasedOn");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapsetStoryboarders_OsuUserId",
                table: "BeatmapsetStoryboarders",
                column: "OsuUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapsetTags_TagsId",
                table: "BeatmapsetTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_OsuUserRoles_RolesId",
                table: "OsuUserRoles",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitCompletions_LearnerId_UnitId",
                table: "UnitCompletions",
                columns: new[] { "LearnerId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatmapsetStoryboarders");

            migrationBuilder.DropTable(
                name: "BeatmapsetTags");

            migrationBuilder.DropTable(
                name: "OsuUserRoles");

            migrationBuilder.DropTable(
                name: "UnitCompletions");

            migrationBuilder.DropTable(
                name: "Beatmapsets");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Learners");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
