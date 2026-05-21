using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class ProcessInfoConfiguration : IEntityTypeConfiguration<ProcessInfo>
{
    public void Configure(EntityTypeBuilder<ProcessInfo> builder)
    {
        builder.ToTable("processes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(512).IsRequired();
        builder.Property(p => p.ExecutablePath).HasMaxLength(1024);
        builder.Property(p => p.CommandLine).HasMaxLength(2048);
        builder.Property(p => p.Pid).IsRequired();
        builder.Property(p => p.StartTime).IsRequired();

        builder.HasIndex(p => p.ScanResultId);
        builder.HasIndex(p => p.Name);
    }
}