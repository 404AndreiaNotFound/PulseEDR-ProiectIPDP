using PulseEDR.Application.Services.Abstractions;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// What-if simulator endpoint — recomputes the risk score on a scan after
/// dismissing one or more alerts. Powers the "what would happen if I fix X?"
/// workflow in the desktop client.
///
/// Routes:
///   POST   /api/whatif/{scanId}
/// Body:    [ "guid1", "guid2", ... ]  (alert ids to dismiss)
/// </summary>
public static class WhatIfEndpoints
{
    public static IEndpointRouteBuilder MapWhatIfEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/whatif")
            .WithTags("What-If")
            .WithOpenApi();

        group.MapPost("/{scanId:guid}", async (
            Guid scanId,
            List<Guid> dismissedAlertIds,
            IWhatIfService whatIf,
            CancellationToken ct) =>
        {
            try
            {
                var result = await whatIf.SimulateAsync(
                    scanId, dismissedAlertIds ?? new List<Guid>(), ct);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("SimulateWhatIf")
        .WithSummary("Recomputes the risk score assuming the listed alerts are dismissed.")
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}