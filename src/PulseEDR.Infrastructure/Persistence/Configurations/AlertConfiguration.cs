using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.ValueObjects;
using System.Text.Json;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Category).HasConversion<int>();
        builder.Property(a => a.Severity).HasConversion<int>();
        builder.Property(a => a.Title).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2048).IsRequired();
        builder.Property(a => a.Remediation).HasMaxLength(2048).IsRequired();
        builder.Property(a => a.CveId).HasMaxLength(32);
        builder.Property(a => a.Score).IsRequired();

        // Evidence value object — serialized as JSONB column for flexibility.
        builder.Property(a => a.Evidence)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Evidence>(v, (JsonSerializerOptions?)null)!
            );

        builder.HasIndex(a => a.ScanResultId);
        builder.HasIndex(a => a.Severity);
        builder.HasIndex(a => a.Category);
    }
}