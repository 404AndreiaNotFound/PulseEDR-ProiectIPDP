using Microsoft.EntityFrameworkCore;
using PulseEDR.Application.Dtos;
using PulseEDR.Application.Mappings;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Infrastructure.Persistence;

namespace PulseEDR.Application.Services.Implementations;

/// <summary>
/// Read-only alerts API. Filtering happens at the SQL level via EF.
/// </summary>
public class AlertService : IAlertService
{
    private readonly PulseEdrDbContext _db;

    public AlertService(PulseEdrDbContext db) => _db = db;

    public async Task<PaginatedResult<AlertDto>> ListAsync(
        Guid? scanId = null,
        Severity? minSeverity = null,
        AlertCategory? category = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        IQueryable<Alert> q = _db.Alerts.AsNoTracking();

        if (scanId.HasValue)        q = q.Where(a => a.ScanResultId == scanId);
        if (minSeverity.HasValue)   q = q.Where(a => a.Severity >= minSeverity);
        if (category.HasValue)      q = q.Where(a => a.Category == category);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(a => a.ToDto()).ToList();
        return new PaginatedResult<AlertDto>(dtos, total, page, pageSize);
    }

    public async Task<AlertDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _db.Alerts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        return alert?.ToDto();
    }
}