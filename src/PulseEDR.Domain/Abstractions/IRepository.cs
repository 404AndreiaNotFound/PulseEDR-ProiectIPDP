using PulseEDR.Domain.Common;
using System.Linq.Expressions;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Generic Repository contract. Abstracts persistence so domain/application
/// layers depend on an interface, not on EF Core (Dependency Inversion).
/// </summary>
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);

    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    void Update(T entity);

    void Remove(T entity);

    Task<int> CountAsync(CancellationToken ct = default);
}