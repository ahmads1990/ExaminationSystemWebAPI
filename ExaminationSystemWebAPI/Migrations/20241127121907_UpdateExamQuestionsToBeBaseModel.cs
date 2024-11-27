using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystemWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamQuestionsToBeBaseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ExamQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ExamQuestions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ExamQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ExamQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ExamQuestions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ExamQuestions");
        }
    }
}
