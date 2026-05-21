using Microsoft.EntityFrameworkCore;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;

namespace PulseEDR.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for the ScanResult aggregate. Includes eager-loading helpers
/// for child collections (processes, connections, software, files, alerts).
/// </summary>
public class ScanResultRepository : Repository<ScanResult>, IScanResultRepository
{
    public ScanResultRepository(PulseEdrDbContext db) : base(db) { }

    public Task<ScanResult?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        Set
            .Include(s => s.Processes)
            .Include(s => s.Connections)
            .Include(s => s.Software)
            .Include(s => s.RecentFiles)
            .Include(s => s.Alerts)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<ScanResult>> GetRecentAsync(int take, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .OrderByDescending(s => s.StartedAt)
            .Take(take)
            .ToListAsync(ct);

    public Task<ScanResult?> GetLatestCompletedAsync(CancellationToken ct = default) =>
        Set
            .AsNoTracking()
            .Where(s => s.Status == ScanStatus.Completed)
            .OrderByDescending(s => s.CompletedAt)
            .FirstOrDefaultAsync(ct);
}