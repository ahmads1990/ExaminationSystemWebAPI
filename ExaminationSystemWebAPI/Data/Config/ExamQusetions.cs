using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Models.Joins;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemWebAPI.Data.Config;

public static class ExamQusetions
{
    public static void ConfigureExamQusetions(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamQuestions>()
           .HasKey(eq => eq.ID);

        modelBuilder.Entity<Exam>()
            .HasMany(e => e.Questions)
            .WithMany(q => q.Exams)
            .UsingEntity<ExamQuestions>();
    }
}
