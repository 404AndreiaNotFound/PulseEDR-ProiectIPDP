namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Unit of Work pattern: commits all repository changes in a single transaction.
/// Decouples "what changed" from "when to persist".
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}