using ExaminationSystemWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemWebAPI.Data.Config;

public static class ExamConfig
{
    public static void ConfigureExam(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exam>()
       .Property(e => e.ExamType)
       .HasConversion<byte>();
    }
}
