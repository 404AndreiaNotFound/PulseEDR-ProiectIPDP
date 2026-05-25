using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class CveEntryConfiguration : IEntityTypeConfiguration<CveEntry>
{
    public void Configure(EntityTypeBuilder<CveEntry> builder)
    {
        builder.ToTable("cves");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CveId).HasMaxLength(32).IsRequired();
        builder.Property(c => c.ProductName).HasMaxLength(256).IsRequired();
        builder.Property(c => c.MaxAffectedVersionExclusive).HasMaxLength(64).IsRequired();
        builder.Property(c => c.FixedVersion).HasMaxLength(64).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(4096).IsRequired();
        builder.Property(c => c.Remediation).HasMaxLength(2048).IsRequired();
        builder.Property(c => c.ReferenceUrl).HasMaxLength(1024);
        builder.Property(c => c.Severity).HasConversion<int>();
        builder.Property(c => c.CvssScore);
        builder.Property(c => c.PublishedAt).IsRequired();

        builder.HasIndex(c => c.CveId).IsUnique();
        builder.HasIndex(c => c.ProductName);
    }
}