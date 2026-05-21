using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class InstalledSoftwareConfiguration : IEntityTypeConfiguration<InstalledSoftware>
{
    public void Configure(EntityTypeBuilder<InstalledSoftware> builder)
    {
        builder.ToTable("software");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(512).IsRequired();
        builder.Property(s => s.Version).HasMaxLength(128).IsRequired();
        builder.Property(s => s.Publisher).HasMaxLength(256);
        builder.Property(s => s.InstallLocation).HasMaxLength(1024);

        builder.HasIndex(s => s.ScanResultId);
        builder.HasIndex(s => s.Name);
    }
}