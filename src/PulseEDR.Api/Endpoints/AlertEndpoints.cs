using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Enums;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// REST endpoints for the Alert resource.
/// Routes:
///   GET    /api/alerts            → list alerts with optional filters
///   GET    /api/alerts/{id}       → single alert with full evidence
/// </summary>
public static class AlertEndpoints
{
    public static IEndpointRouteBuilder MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alerts")
            .WithTags("Alerts")
            .WithOpenApi();

        group.MapGet("/", async (
            IAlertService alerts,
            Guid? scanId = null,
            Severity? minSeverity = null,
            AlertCategory? category = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var result = await alerts.ListAsync(
                scanId, minSeverity, category, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("ListAlerts")
        .WithSummary("Lists alerts with optional filters by scan, severity, and category.");

        group.MapGet("/{id:guid}", async (
            Guid id, IAlertService alerts, CancellationToken ct) =>
        {
            var result = await alerts.GetByIdAsync(id, ct);
            return result is null
                ? Results.NotFound(new { message = $"Alert {id} not found." })
                : Results.Ok(result);
        })
        .WithName("GetAlertById")
        .WithSummary("Returns a single alert with full evidence and remediation.")
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}