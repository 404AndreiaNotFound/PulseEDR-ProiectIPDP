using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class ScanResultConfiguration : IEntityTypeConfiguration<ScanResult>
{
    public void Configure(EntityTypeBuilder<ScanResult> builder)
    {
        builder.ToTable("scans");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.MachineName).HasMaxLength(256).IsRequired();
        builder.Property(s => s.OsVersion).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Status).HasConversion<int>();

        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.CompletedAt);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);

        // RiskScore (value object) — stored as owned columns.
        builder.OwnsOne(s => s.RiskScore, vo =>
        {
            vo.Property(r => r.Value).HasColumnName("risk_score_value");
            vo.Property(r => r.Severity)
              .HasColumnName("risk_score_severity")
              .HasConversion<int>();
        });

        // Child collections — backing field access (entities use private List<>).
        builder.HasMany(s => s.Processes)
               .WithOne()
               .HasForeignKey(p => p.ScanResultId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Connections)
               .WithOne()
               .HasForeignKey(c => c.ScanResultId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Software)
               .WithOne()
               .HasForeignKey(sw => sw.ScanResultId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.RecentFiles)
               .WithOne()
               .HasForeignKey(f => f.ScanResultId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Alerts)
               .WithOne()
               .HasForeignKey(a => a.ScanResultId)
               .OnDelete(DeleteBehavior.Cascade);

        // Tell EF Core to use the backing fields for the child collections.
        builder.Navigation(s => s.Processes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(s => s.Connections).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(s => s.Software).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(s => s.RecentFiles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(s => s.Alerts).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => s.StartedAt);
        builder.HasIndex(s => s.Status);
    }
}