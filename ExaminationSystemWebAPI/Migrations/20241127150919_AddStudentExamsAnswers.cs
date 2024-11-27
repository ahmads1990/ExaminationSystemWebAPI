using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystemWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentExamsAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentExamsAnswers",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExamID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuestionID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChoiceID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExamsAnswers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentExamsAnswers_Choices_ChoiceID",
                        column: x => x.ChoiceID,
                        principalTable: "Choices",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentExamsAnswers_Exams_ExamID",
                        column: x => x.ExamID,
                        principalTable: "Exams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentExamsAnswers_Questions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Questions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentExamsAnswers_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamsAnswers_ChoiceID",
                table: "StudentExamsAnswers",
                column: "ChoiceID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamsAnswers_ExamID",
                table: "StudentExamsAnswers",
                column: "ExamID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamsAnswers_QuestionID",
                table: "StudentExamsAnswers",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamsAnswers_StudentID",
                table: "StudentExamsAnswers",
                column: "StudentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentExamsAnswers");
        }
    }
}
