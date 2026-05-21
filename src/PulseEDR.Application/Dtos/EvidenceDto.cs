namespace PulseEDR.Application.Dtos;

/// <summary>
/// Flat shape of Evidence for API responses. Mirrors the value object
/// but uses plain Dictionary for serialization.
/// </summary>
public sealed record EvidenceDto(
    string DetectorName,
    string Description,
    Dictionary<string, string> Facts);