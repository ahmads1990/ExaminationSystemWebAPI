using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class ExamConfig
{
    public static void ConfigureExam(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exam>()
            .Property(e => e.PassingScore)
            .HasPrecision(18, 2);
    }
}
