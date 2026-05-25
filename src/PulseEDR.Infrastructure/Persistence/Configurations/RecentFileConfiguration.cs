using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class RecentFileConfiguration : IEntityTypeConfiguration<RecentFile>
{
    public void Configure(EntityTypeBuilder<RecentFile> builder)
    {
        builder.ToTable("recent_files");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FullPath).HasMaxLength(1024).IsRequired();
        builder.Property(f => f.FileName).HasMaxLength(512).IsRequired();
        builder.Property(f => f.Extension).HasMaxLength(32).IsRequired();
        builder.Property(f => f.Sha256).HasMaxLength(128);

        builder.HasIndex(f => f.ScanResultId);
        builder.HasIndex(f => f.Extension);
    }
}