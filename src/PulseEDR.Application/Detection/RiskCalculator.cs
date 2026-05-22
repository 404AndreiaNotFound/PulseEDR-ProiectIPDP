using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Detection;

/// <summary>
/// Aggregates Alert.Score values into a final RiskScore (clamped 0-100).
///
/// Separation of concerns (SRP): detectors decide WHAT is risky and assign a
/// per-alert score; this calculator decides HOW to combine them into the
/// final number. Easy to swap with a fancier algorithm (weighted average,
/// Bayesian, etc.) without touching detectors.
/// </summary>
public class RiskCalculator : IRiskCalculator
{
    public RiskScore Calculate(IReadOnlyCollection<Alert> alerts)
    {
        if (alerts.Count == 0)
            return RiskScore.Zero;

        // Base aggregation: sum of per-alert scores, capped at 100.
        var raw = alerts.Sum(a => a.Score);

        // Optional small boost when multiple critical/high alerts are present
        // (defense-in-depth: many medium signals = higher confidence overall).
        var criticalCount = alerts.Count(a => a.Severity == Severity.Critical);
        var highCount = alerts.Count(a => a.Severity == Severity.High);

        if (criticalCount >= 2) raw += 5;
        if (highCount >= 3) raw += 5;

        var clamped = Math.Clamp(raw, 0, 100);
        return new RiskScore(clamped);
    }
}