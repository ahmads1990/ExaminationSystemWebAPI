using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Infrastructure.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    #region Services

    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantAccessor _tenantAccessor;

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
    public DbSet<ExamAttempt> ExamAttempts { get; set; }
    public DbSet<ExamQuestion> ExamQuestions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantDomain> TenantDomains { get; set; }

    #endregion

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService, ITenantAccessor tenantAccessor) : base(options)
    {
        _currentUserService = currentUserService;
        _tenantAccessor = tenantAccessor;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int? currentUserId = _currentUserService.UserId;
        int? currentTenantId = _tenantAccessor.TenantId;

        foreach (var entry in ChangeTracker.Entries<BaseModel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUserId;
                entry.Entity.CreatedDate = DateTime.UtcNow;

                // Auto-set TenantId only if not already explicitly set
                if (entry.Entity.TenantId == 0 && currentTenantId.HasValue)
                    entry.Entity.TenantId = currentTenantId.Value;
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

        // Apply global tenant query filter to all BaseModel entities
        var currentTenantId = _tenantAccessor.TenantId;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseModel).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplyTenantQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder, currentTenantId });
            }
        }

        modelBuilder.ApplyConfiguration(new TenantDomainConfiguration());

        modelBuilder.ConfigureQuestionChoices();
        modelBuilder.ConfigureStudentCourses();
        modelBuilder.ConfigureStudentExamsAnswers();
        modelBuilder.ConfigureAppUserStudent();
        modelBuilder.ConfigureAppUserInstructor();
        modelBuilder.ConfigureExamAttempt();
        modelBuilder.ConfigureExam(); 
    }

    private static void ApplyTenantQueryFilter<T>(ModelBuilder modelBuilder, int? tenantId) where T : BaseModel
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !tenantId.HasValue || e.TenantId == tenantId.Value);
    }
}
