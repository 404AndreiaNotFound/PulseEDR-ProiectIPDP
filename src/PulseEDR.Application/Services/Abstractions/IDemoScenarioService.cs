using PulseEDR.Application.Dtos;

namespace PulseEDR.Application.Services.Abstractions;

/// <summary>
/// Runs a curated "threat chain" scenario that demonstrates PulseEDR's full
/// detection pipeline in a single click. Used in live demos to show:
///   - file landed in Downloads (UnsafeDownload)
///   - process started from Downloads (SuspiciousProcess)
///   - outbound to public IP on uncommon port (RiskyConnection)
///   - vulnerable Chrome installed (OutdatedSoftware + CVE match)
///
/// This is a key differentiator and the centerpiece of the project demo.
/// </summary>
public interface IDemoScenarioService
{
    /// <summary>
    /// Runs the canonical threat chain scenario and returns the resulting scan
    /// (with alerts and final risk score). Persists everything to the DB.
    /// </summary>
    Task<ScanDetailDto> RunThreatChainAsync(CancellationToken ct = default);
}