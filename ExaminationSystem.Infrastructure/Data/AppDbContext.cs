using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Infrastructure.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    #region Entities

    public DbSet<Course> Courses { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }

    #endregion

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureQuestionChoices();
    }
}
