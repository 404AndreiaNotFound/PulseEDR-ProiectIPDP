using PulseEDR.Domain.Enums;

namespace PulseEDR.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a computed risk score (0-100)
/// with a derived severity label. Compares by content, not identity.
/// </summary>
public sealed record RiskScore
{
    public int Value { get; }
    public Severity Severity { get; }

    public RiskScore(int value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value),
                "Risk score must be between 0 and 100.");

        Value = value;
        Severity = DeriveSeverity(value);
    }

    private static Severity DeriveSeverity(int value) => value switch
    {
        < 20 => Severity.None,
        < 40 => Severity.Low,
        < 60 => Severity.Medium,
        < 80 => Severity.High,
        _    => Severity.Critical
    };

    public static RiskScore Zero => new(0);
    public static RiskScore Max  => new(100);

    public override string ToString() => $"{Value} ({Severity})";
}