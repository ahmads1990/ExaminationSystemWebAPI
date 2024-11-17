using ExaminationSystemWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemWebAPI.Data;

public class AppDbContext : DbContext
{
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
    }
}
