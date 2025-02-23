using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class StudentExamsAnswersConfig
{
    public static void ConfigureStudentExamsAnswers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentExamsAnswers>()
            .HasOne(sea => sea.Student)
            .WithMany(s => s.StudentExamsAnswers)
            .HasForeignKey(sea => sea.StudentID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StudentExamsAnswers>()
            .HasOne(sea => sea.Exam)
            .WithMany()
            .HasForeignKey(sea => sea.ExamID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentExamsAnswers>()
            .HasOne(sea => sea.Question)
            .WithMany()
            .HasForeignKey(sea => sea.QuestionID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StudentExamsAnswers>()
            .HasOne(sea => sea.Choice)
            .WithMany()
            .HasForeignKey(sea => sea.ChoiceID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
