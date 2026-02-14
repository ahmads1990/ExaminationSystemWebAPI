using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Infrastructure.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    #region Services

    private readonly ICurrentUserService _currentUserService;

    #endregion

    #region Entities

    public DbSet<Course> Courses { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    #endregion

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int? currentUserId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseModel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUserId;
                entry.Entity.CreatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedBy = currentUserId;
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureQuestionChoices();
        modelBuilder.ConfigureStudentCourses();
        modelBuilder.ConfigureStudentExamsAnswers();
        modelBuilder.ConfigureAppUserStudent();
        modelBuilder.ConfigureAppUserInstructor();
    }
}
