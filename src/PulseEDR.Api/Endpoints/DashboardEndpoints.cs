using PulseEDR.Application.Services.Abstractions;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// Dashboard summary endpoint — feeds the WPF home screen.
/// Routes:
///   GET    /api/dashboard/summary
/// </summary>
public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .WithOpenApi();

        group.MapGet("/summary", async (
            IDashboardService dashboard, CancellationToken ct) =>
        {
            var result = await dashboard.GetSummaryAsync(ct);
            return Results.Ok(result);
        })
        .WithName("GetDashboardSummary")
        .WithSummary("High-level summary for the Dashboard view in the desktop client.");

        return app;
    }
}