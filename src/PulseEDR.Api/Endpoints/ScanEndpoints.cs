using PulseEDR.Application.Services.Abstractions;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// REST endpoints for the Scan resource.
/// Routes:
///   POST   /api/scan                  → trigger a new scan
///   GET    /api/scan                  → list recent scans (paginated)
///   GET    /api/scan/{id}             → get scan details with all child data
///   GET    /api/scan/latest           → most recently completed scan
/// </summary>
public static class ScanEndpoints
{
    public static IEndpointRouteBuilder MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scan")
            .WithTags("Scans")
            .WithOpenApi();

        group.MapPost("/", async (IScanService scans, CancellationToken ct) =>
        {
            var result = await scans.RunScanAsync(ct);
            return Results.Created($"/api/scan/{result.Id}", result);
        })
        .WithName("RunScan")
        .WithSummary("Triggers a new scan and returns the full result.")
        .Produces(StatusCodes.Status201Created);

        group.MapGet("/", async (
            IScanService scans,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await scans.ListRecentAsync(page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("ListRecentScans")
        .WithSummary("Returns a paginated list of recent scans.");

        group.MapGet("/latest", async (IScanService scans, CancellationToken ct) =>
        {
            var result = await scans.GetLatestCompletedAsync(ct);
            return result is null
                ? Results.NotFound(new { message = "No completed scan yet." })
                : Results.Ok(result);
        })
        .WithName("GetLatestScan")
        .WithSummary("Returns the most recently completed scan, with full details.")
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", async (
            Guid id, IScanService scans, CancellationToken ct) =>
        {
            var result = await scans.GetByIdAsync(id, ct);
            return result is null
                ? Results.NotFound(new { message = $"Scan {id} not found." })
                : Results.Ok(result);
        })
        .WithName("GetScanById")
        .WithSummary("Returns a single scan with full details.")
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}