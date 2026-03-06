using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShowResultsImmediatelyFromExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowResultsImmediately",
                table: "Exams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsImmediately",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
