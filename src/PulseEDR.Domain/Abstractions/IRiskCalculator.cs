using PulseEDR.Domain.Entities;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Aggregates Alert scores into a final RiskScore.
/// Decoupled from detectors (Single Responsibility): detectors find issues,
/// calculator weighs them.
/// </summary>
public interface IRiskCalculator
{
    RiskScore Calculate(IReadOnlyCollection<Alert> alerts);
}