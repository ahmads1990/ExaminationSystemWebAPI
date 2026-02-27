using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class ExamAttemptConfig
{
    public static void ConfigureExamAttempt(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamAttempt>()
            .HasOne(a => a.Student)
            .WithMany(s => s.ExamAttempts)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ExamAttempt>()
            .HasOne(a => a.Exam)
            .WithMany(e => e.ExamAttempts)
            .HasForeignKey(a => a.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
