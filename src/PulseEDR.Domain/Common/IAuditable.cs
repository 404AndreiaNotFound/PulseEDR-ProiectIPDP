namespace PulseEDR.Domain.Common;

/// <summary>
/// Marker for entities that track creation and modification timestamps.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}