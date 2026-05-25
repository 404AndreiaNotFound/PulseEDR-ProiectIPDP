using PulseEDR.Application.Dtos;
using PulseEDR.Domain.Enums;

namespace PulseEDR.Application.Services.Abstractions;

/// <summary>
/// Read-only access to alerts (with filtering for the Alerts view).
/// </summary>
public interface IAlertService
{
    Task<PaginatedResult<AlertDto>> ListAsync(
        Guid? scanId = null,
        Severity? minSeverity = null,
        AlertCategory? category = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);

    Task<AlertDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}