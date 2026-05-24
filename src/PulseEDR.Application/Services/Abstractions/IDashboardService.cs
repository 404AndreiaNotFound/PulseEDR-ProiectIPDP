using PulseEDR.Application.Dtos;

namespace PulseEDR.Application.Services.Abstractions;

/// <summary>
/// Builds the high-level summary shown on the WPF Dashboard view.
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}