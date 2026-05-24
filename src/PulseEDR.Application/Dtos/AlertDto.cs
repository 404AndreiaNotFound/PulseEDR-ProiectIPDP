namespace PulseEDR.Application.Dtos;

/// <summary>
/// Outgoing representation of an Alert. Includes everything the UI needs
/// to display the alert and its explainability data.
/// </summary>
public sealed record AlertDto(
    Guid Id,
    Guid ScanResultId,
    string Category,
    string Severity,
    int Score,
    string Title,
    string Description,
    string Remediation,
    EvidenceDto Evidence,
    string? CveId,
    DateTime CreatedAt);