using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class StudentCoursesConfig
{
    public static void ConfigureStudentCourses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentCourses>()
            .HasOne(sc => sc.Student)
            .WithMany(s => s.StudentCourses)
            .HasForeignKey(sc => sc.StudentID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

