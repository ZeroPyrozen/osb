using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace osb.Data.Migrations
{
    /// <summary>
    /// Keeps the score of each learner's first attempt at a knowledge check: the perfect-score bonus is
    /// only for a perfect first attempt. Existing completions get null and keep the bonus they had.
    /// </summary>
    public partial class QuizFirstScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirstScore",
                table: "UnitCompletions",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstScore",
                table: "UnitCompletions");
        }
    }
}
