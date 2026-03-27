using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExaminationSystem.Infrastructure.Data.Config;

public class TenantDomainConfiguration : IEntityTypeConfiguration<TenantDomain>
{
    public void Configure(EntityTypeBuilder<TenantDomain> builder)
    {
        builder.HasKey(td => td.ID);

        builder.Property(td => td.Domain)
            .IsRequired()
            .HasMaxLength(253); // RFC 1035 max domain length

        builder.HasIndex(td => td.Domain)
            .IsUnique();

        builder.HasOne(td => td.Tenant)
            .WithMany(t => t.Domains)
            .HasForeignKey(td => td.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
