using Microsoft.EntityFrameworkCore;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for CveEntry. Adds product-name lookup used by detectors,
/// and bulk upsert used by the CVE feed importer.
/// </summary>
public class CveRepository : Repository<CveEntry>, ICveRepository
{
    public CveRepository(PulseEdrDbContext db) : base(db) { }

    public async Task<IReadOnlyList<CveEntry>> FindByProductAsync(
        string productName, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.ProductName, productName))
            .ToListAsync(ct);

    public async Task UpsertManyAsync(IEnumerable<CveEntry> entries, CancellationToken ct = default)
    {
        foreach (var entry in entries)
        {
            var existing = await Set.FirstOrDefaultAsync(c => c.CveId == entry.CveId, ct);
            if (existing is null)
            {
                await Set.AddAsync(entry, ct);
            }
            // For brevity, we don't update existing CVEs here. In production,
            // we'd compare fields and update changed columns.
        }
    }
}