using PulseEDR.Application.Services.Abstractions;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// Demo scenario endpoint — runs the canonical threat-chain scenario for
/// live demonstrations. Hand-crafted realistic telemetry, real detection
/// pipeline. Single click → full picture of the system at work.
///
/// Routes:
///   POST   /api/demo/threat-chain
/// </summary>
public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demo")
            .WithTags("Demo")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/threat-chain", async (
            IDemoScenarioService demo, CancellationToken ct) =>
        {
            var result = await demo.RunThreatChainAsync(ct);
            return Results.Created($"/api/scan/{result.Id}", result);
        })
        .WithName("RunDemoThreatChain")
        .WithSummary(
            "Runs the canonical threat-chain demo scenario " +
            "(unsafe download + suspicious process + risky connection + outdated software).")
        .Produces(StatusCodes.Status201Created);

        return app;
    }
}