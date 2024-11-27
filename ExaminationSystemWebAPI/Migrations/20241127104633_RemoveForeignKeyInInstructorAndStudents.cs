using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystemWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveForeignKeyInInstructorAndStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_AspNetUsers_AppUserID",
                table: "Instructors");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_AppUserID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_AppUserID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Instructors_AppUserID",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "AppUserID",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AppUserID",
                table: "Instructors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserID",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AppUserID",
                table: "Instructors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AppUserID",
                table: "Students",
                column: "AppUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_AppUserID",
                table: "Instructors",
                column: "AppUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_AspNetUsers_AppUserID",
                table: "Instructors",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_AppUserID",
                table: "Students",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
