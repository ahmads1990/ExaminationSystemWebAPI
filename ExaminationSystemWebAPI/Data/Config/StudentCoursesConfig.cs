using ExaminationSystemWebAPI.Models.Joins;
using ExaminationSystemWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemWebAPI.Data.Config;

public static class StudentCoursesConfig
{
    public static void ConfigureStudentCourses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentCourses>()
           .HasKey(eq => eq.ID);

        modelBuilder.Entity<Course>()
            .HasMany(c=>c.Students)
            .WithMany(s=>s.Courses)
            .UsingEntity<StudentCourses>();
    }
}
