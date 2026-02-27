using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_Exams_ExamId",
                table: "ExamQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_Questions_QuestionId",
                table: "ExamQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExamsAnswers_Exams_ExamID",
                table: "StudentExamsAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamQuestion",
                table: "ExamQuestion");

            migrationBuilder.RenameTable(
                name: "ExamQuestion",
                newName: "ExamQuestions");

            migrationBuilder.RenameColumn(
                name: "ExamID",
                table: "StudentExamsAnswers",
                newName: "ExamAttemptID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentExamsAnswers_ExamID",
                table: "StudentExamsAnswers",
                newName: "IX_StudentExamsAnswers_ExamAttemptID");

            migrationBuilder.RenameColumn(
                name: "PassMark",
                table: "Exams",
                newName: "MaxDurationInMinutes");

            migrationBuilder.RenameColumn(
                name: "MaxDuration",
                table: "Exams",
                newName: "MaxAttempts");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "Exams",
                newName: "ShuffleQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestion_QuestionId",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestion_ExamId",
                table: "ExamQuestions",
                newName: "IX_ExamQuestions_ExamId");

            migrationBuilder.AddColumn<int>(
                name: "ExamStatus",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingScore",
                table: "Exams",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDate",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsImmediately",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamQuestions",
                table: "ExamQuestions",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "ExamAttempts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamAttemptStatus = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttempts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExamAttempts_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_ExamId",
                table: "ExamAttempts",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_StudentId",
                table: "ExamAttempts",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Exams_ExamId",
                table: "ExamQuestions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Questions_QuestionId",
                table: "ExamQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExamsAnswers_ExamAttempts_ExamAttemptID",
                table: "StudentExamsAnswers",
                column: "ExamAttemptID",
                principalTable: "ExamAttempts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Exams_ExamId",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Questions_QuestionId",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExamsAnswers_ExamAttempts_ExamAttemptID",
                table: "StudentExamsAnswers");

            migrationBuilder.DropTable(
                name: "ExamAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamQuestions",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "ExamStatus",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ShowResultsImmediately",
                table: "Exams");

            migrationBuilder.RenameTable(
                name: "ExamQuestions",
                newName: "ExamQuestion");

            migrationBuilder.RenameColumn(
                name: "ExamAttemptID",
                table: "StudentExamsAnswers",
                newName: "ExamID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentExamsAnswers_ExamAttemptID",
                table: "StudentExamsAnswers",
                newName: "IX_StudentExamsAnswers_ExamID");

            migrationBuilder.RenameColumn(
                name: "ShuffleQuestions",
                table: "Exams",
                newName: "IsPublished");

            migrationBuilder.RenameColumn(
                name: "MaxDurationInMinutes",
                table: "Exams",
                newName: "PassMark");

            migrationBuilder.RenameColumn(
                name: "MaxAttempts",
                table: "Exams",
                newName: "MaxDuration");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_QuestionId",
                table: "ExamQuestion",
                newName: "IX_ExamQuestion_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestions_ExamId",
                table: "ExamQuestion",
                newName: "IX_ExamQuestion_ExamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamQuestion",
                table: "ExamQuestion",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_Exams_ExamId",
                table: "ExamQuestion",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_Questions_QuestionId",
                table: "ExamQuestion",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExamsAnswers_Exams_ExamID",
                table: "StudentExamsAnswers",
                column: "ExamID",
                principalTable: "Exams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
