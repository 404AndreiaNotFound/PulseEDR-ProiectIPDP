namespace PulseEDR.Domain.ValueObjects;

/// <summary>
/// Immutable record describing WHY an alert was raised.
/// Powers the explainability feature (mapped from prototype's "Reasons" list).
/// </summary>
public sealed record Evidence(
    string DetectorName,
    string Description,
    IReadOnlyDictionary<string, string> Facts
)
{
    public static Evidence Empty(string detectorName) =>
        new(detectorName, "No supporting evidence collected.",
            new Dictionary<string, string>());
}