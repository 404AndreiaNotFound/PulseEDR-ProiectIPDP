using PulseEDR.Domain.Abstractions;

namespace PulseEDR.Infrastructure.Persistence.Repositories;

/// <summary>
/// Wraps DbContext.SaveChangesAsync as a domain-friendly operation.
/// Repositories accumulate changes; UnitOfWork commits them atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PulseEdrDbContext _db;

    public UnitOfWork(PulseEdrDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}