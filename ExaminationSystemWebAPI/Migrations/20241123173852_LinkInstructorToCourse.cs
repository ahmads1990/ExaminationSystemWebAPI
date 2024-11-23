using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystemWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class LinkInstructorToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorID",
                table: "Course",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Course_InstructorID",
                table: "Course",
                column: "InstructorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Instructors_InstructorID",
                table: "Course",
                column: "InstructorID",
                principalTable: "Instructors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Instructors_InstructorID",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Course_InstructorID",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "InstructorID",
                table: "Course");
        }
    }
}
