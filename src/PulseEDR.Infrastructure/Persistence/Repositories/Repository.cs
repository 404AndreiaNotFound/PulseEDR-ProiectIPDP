using Microsoft.EntityFrameworkCore;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Common;
using System.Linq.Expressions;

namespace PulseEDR.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic Repository base. Specialized repositories inherit from this
/// to inherit CRUD-like behavior, and add domain-specific queries.
/// </summary>
public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly PulseEdrDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(PulseEdrDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public Task AddAsync(T entity, CancellationToken ct = default) =>
        Set.AddAsync(entity, ct).AsTask();

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);

    public Task<int> CountAsync(CancellationToken ct = default) =>
        Set.CountAsync(ct);
}