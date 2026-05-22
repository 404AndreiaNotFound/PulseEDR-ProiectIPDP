using PulseEDR.Domain.Abstractions;

namespace PulseEDR.Application.Detection;

/// <summary>
/// Factory that surfaces all registered IDetector implementations.
/// Allows callers to enumerate detectors without knowing concrete types.
///
/// Pattern: Factory. Plays nicely with DI — concrete detectors are registered
/// once in AddApplication() and consumed here.
/// </summary>
public interface IDetectorFactory
{
    IReadOnlyList<IDetector> GetAll();
}

public class DetectorFactory : IDetectorFactory
{
    private readonly IEnumerable<IDetector> _detectors;

    public DetectorFactory(IEnumerable<IDetector> detectors) => _detectors = detectors;

    public IReadOnlyList<IDetector> GetAll() => _detectors.ToList();
}