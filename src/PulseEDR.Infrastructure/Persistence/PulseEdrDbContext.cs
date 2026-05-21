using Microsoft.EntityFrameworkCore;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for PulseEDR. Applies all IEntityTypeConfiguration classes
/// from this assembly via reflection so the OnModelCreating stays clean.
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
        // Apply all IEntityTypeConfiguration<T> from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PulseEdrDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}