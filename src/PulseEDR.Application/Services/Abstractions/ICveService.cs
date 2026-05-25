using PulseEDR.Application.Dtos;

namespace PulseEDR.Application.Services.Abstractions;

/// <summary>
/// Read-only access to the CVE catalog (plus import trigger for admin/demo).
/// </summary>
public interface ICveService
{
    Task<PaginatedResult<CveDto>> ListAsync(int page = 1, int pageSize = 50,
        CancellationToken ct = default);

    Task<CveDto?> GetByCveIdAsync(string cveId, CancellationToken ct = default);

    /// <summary>
    /// Triggers an import from the local CVE catalog file (cves.json).
    /// Used at startup and on-demand by the admin endpoint.
    /// </summary>
    Task<int> ImportFromLocalCatalogAsync(CancellationToken ct = default);
}