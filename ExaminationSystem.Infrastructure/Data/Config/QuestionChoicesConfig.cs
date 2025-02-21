using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Infrastructure.Data.Config;

public static class QuestionChoicesConfig
{
    public static void ConfigureQuestionChoices(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Question>()
         .HasMany(q => q.Choices)
         .WithOne(c => c.Question)
         .HasForeignKey(c => c.QuestionId);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.Answer)
            .WithOne()
            .HasForeignKey<Question>(q => q.AnswerId);
    }
}
