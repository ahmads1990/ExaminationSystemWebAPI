using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class AppUserStudentConfig
{
    public static void ConfigureAppUserStudent(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.HasOne(s => s.AppUser)
                  .WithOne(au => au.Student)
                  .HasForeignKey<Student>(s => s.ID)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(s => s.ID)
                  .ValueGeneratedNever();
        });
    }
}
