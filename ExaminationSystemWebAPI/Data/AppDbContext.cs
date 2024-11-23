using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.Models.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ExaminationSystemWebAPI.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Choice>()
            .Property(c => c.ChoiceOrder)
            .HasConversion<byte>();

        modelBuilder.Entity<Exam>()
          .Property(e => e.ExamType)
          .HasConversion<byte>();

        // Exam <-> Questions
        modelBuilder.Entity<ExamQuestions>()
            .HasKey(eq => eq.ID);

        modelBuilder.Entity<Exam>()
            .HasMany(e => e.Questions)
            .WithMany(q => q.Exams)
            .UsingEntity<ExamQuestions>();
    }
}
