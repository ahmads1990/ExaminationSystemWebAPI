using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class AppUserInstructorConfig
{
    public static void ConfigureAppUserInstructor(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.HasOne(i => i.AppUser)
                  .WithOne(au => au.Instructor)
                  .HasForeignKey<Instructor>(i => i.ID)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(i => i.ID)
                  .ValueGeneratedNever();
        });
    }
}
