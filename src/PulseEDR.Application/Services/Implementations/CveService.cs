using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseEDR.Application.Dtos;
using PulseEDR.Application.Mappings;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Infrastructure.Persistence;

namespace PulseEDR.Application.Services.Implementations;

/// <summary>
/// CVE catalog read access + import orchestration across multiple sources.
/// Uses the Adapter pattern (ICveFeedSource) to fetch from local catalog
/// and remote NVD API. Sources are queried in parallel and merged by CveId.
/// </summary>
public class CveService : ICveService
{
    private readonly ICveRepository _cves;
    private readonly IUnitOfWork _uow;
    private readonly PulseEdrDbContext _db;
    private readonly IEnumerable<ICveFeedSource> _sources;
    private readonly ILogger<CveService> _log;

    public CveService(
        ICveRepository cves,
        IUnitOfWork uow,
        PulseEdrDbContext db,
        IEnumerable<ICveFeedSource> sources,
        ILogger<CveService> log)
    {
        _cves = cves;
        _uow = uow;
        _db = db;
        _sources = sources;
        _log = log;
    }

    public async Task<PaginatedResult<CveDto>> ListAsync(
        int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var total = await _db.Cves.CountAsync(ct);
        var items = await _db.Cves
            .AsNoTracking()
            .OrderByDescending(c => c.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<CveDto>(
            items.Select(c => c.ToDto()).ToList(), total, page, pageSize);
    }

    public async Task<CveDto?> GetByCveIdAsync(string cveId, CancellationToken ct = default)
    {
        var cve = await _db.Cves.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CveId == cveId, ct);
        return cve?.ToDto();
    }

    /// <summary>
    /// Imports CVEs from all configured sources (local JSON + NVD API).
    /// Sources are queried in parallel and merged by CveId (first writer wins).
    /// </summary>
    public async Task<int> ImportFromLocalCatalogAsync(CancellationToken ct = default)
    {
        _log.LogInformation(
            "CVE import: querying {Count} source(s)...", _sources.Count());

        // Fetch from all sources in parallel for speed.
        var fetchTasks = _sources.Select(async s =>
        {
            var entries = await s.FetchAsync(ct);
            _log.LogInformation(
                "CVE source '{Name}' returned {Count} entries.",
                s.SourceName, entries.Count);
            return entries;
        });

        var results = await Task.WhenAll(fetchTasks);

        // Merge by CveId — earlier sources (local catalog) win to keep curated data.
        var merged = results
            .SelectMany(r => r)
            .GroupBy(e => e.CveId)
            .Select(g => g.First())
            .ToList();

        if (merged.Count == 0)
        {
            _log.LogWarning("CVE import: no entries received from any source.");
            return 0;
        }

        await _cves.UpsertManyAsync(merged, ct);
        var saved = await _uow.SaveChangesAsync(ct);

        _log.LogInformation(
            "CVE import: {Count} entries merged from {Sources} source(s); persisted {Saved}.",
            merged.Count, _sources.Count(), saved);

        return saved;
    }
}