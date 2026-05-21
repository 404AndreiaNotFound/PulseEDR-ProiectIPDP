using PulseEDR.Domain.Common;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// A finding produced by a detector. Carries severity, evidence, and
/// remediation. Aggregated and weighted into the final RiskScore.
///
/// Equivalent to legacy AlertItem but with structured Evidence (typed) instead
/// of raw string lists, and category as a typed enum.
/// </summary>
public class Alert : Entity, IAuditable
{
    public Guid ScanResultId { get; private set; }
    public AlertCategory Category { get; private set; }
    public Severity Severity { get; private set; }
    public int Score { get; private set; }  // raw contribution to risk score
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Remediation { get; private set; } = string.Empty;
    public Evidence Evidence { get; private set; } = default!;
    public string? CveId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Alert() { }

    public Alert(AlertCategory category, Severity severity, int score,
                 string title, string description, string remediation,
                 Evidence evidence, string? cveId = null)
    {
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(nameof(score));

        Category = category;
        Severity = severity;
        Score = score;
        Title = title;
        Description = description;
        Remediation = remediation;
        Evidence = evidence;
        CveId = cveId;
        CreatedAt = DateTime.UtcNow;
    }

    internal void AttachToScan(Guid scanResultId) => ScanResultId = scanResultId;
}