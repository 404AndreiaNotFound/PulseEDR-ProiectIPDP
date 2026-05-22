using PulseEDR.Application.Services.Abstractions;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// REST endpoints for the CVE catalog.
/// Routes:
///   GET    /api/cves               → list CVEs (paginated)
///   GET    /api/cves/{cveId}       → single CVE by official id (CVE-YYYY-XXXX)
///   POST   /api/cves/import        → trigger an on-demand import from all sources
/// </summary>
public static class CveEndpoints
{
    public static IEndpointRouteBuilder MapCveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cves")
            .WithTags("CVEs")
            .WithOpenApi();

        group.MapGet("/", async (
            ICveService cves,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var result = await cves.ListAsync(page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("ListCves")
        .WithSummary("Returns the CVE catalog (paginated).");

        group.MapGet("/{cveId}", async (
            string cveId, ICveService cves, CancellationToken ct) =>
        {
            var result = await cves.GetByCveIdAsync(cveId, ct);
            return result is null
                ? Results.NotFound(new { message = $"CVE {cveId} not found." })
                : Results.Ok(result);
        })
        .WithName("GetCveById")
        .WithSummary("Returns one CVE by its official identifier (e.g. CVE-2024-0519).")
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/import", async (ICveService cves, CancellationToken ct) =>
        {
            var imported = await cves.ImportFromLocalCatalogAsync(ct);
            return Results.Ok(new { imported, message = "CVE import completed." });
        })
        .WithName("ImportCves")
        .WithSummary("Triggers an on-demand import from all configured CVE feed sources.");

        return app;
    }
}