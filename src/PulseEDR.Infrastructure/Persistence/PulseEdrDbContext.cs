using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for PulseEDR. Applies all IEntityTypeConfiguration classes
/// from this assembly via reflection so OnModelCreating stays clean.
///
/// Also installs a global value converter that normalizes every DateTime/DateTime?
/// property to UTC before persisting — required by Npgsql 6+ for
/// 'timestamp with time zone' columns.
/// </summary>
public class PulseEdrDbContext : DbContext
{
    public PulseEdrDbContext(DbContextOptions<PulseEdrDbContext> options)
        : base(options) { }

    public DbSet<ScanResult>         Scans          => Set<ScanResult>();
    public DbSet<ProcessInfo>        Processes      => Set<ProcessInfo>();
    public DbSet<NetworkConnection>  Connections    => Set<NetworkConnection>();
    public DbSet<InstalledSoftware>  Software       => Set<InstalledSoftware>();
    public DbSet<RecentFile>         RecentFiles    => Set<RecentFile>();
    public DbSet<Alert>              Alerts         => Set<Alert>();
    public DbSet<CveEntry>           Cves           => Set<CveEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PulseEdrDbContext).Assembly);

        // Global UTC normalization for every DateTime / DateTime? property.
        // Any value with Kind=Unspecified or Local is converted to UTC at save.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue
                ? null
                : (v.Value.Kind == DateTimeKind.Utc
                    ? v.Value
                    : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)),
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}