using ExaminationSystemWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemWebAPI.Data.Config;

public static class ChoiceConfig
{
    public static void ConfigureChoice(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Choice>()
              .Property(c => c.ChoiceOrder)
              .HasConversion<byte>();
    }
}
