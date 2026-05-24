using System.Net.Http.Json;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace PulseEDR.PerformanceTests;

/// <summary>
/// Load test against the /api/scan endpoint. Requires the API to be
/// running locally on http://localhost:5015 with Postgres available.
/// </summary>
public class ScanEndpointLoadTest
{
    private const string BaseUrl = "http://localhost:5015";

    [Fact]
    [Trait("Category", "Performance")]
    public async Task ScanEndpoint_HandlesSustainedLoad()
    {
        // 1. Get JWT
        using var loginClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var loginResp = await loginClient.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "admin" });
        loginResp.EnsureSuccessStatusCode();

        var loginBody = await loginResp.Content.ReadAsStringAsync();
        var token = System.Text.Json.JsonDocument.Parse(loginBody)
            .RootElement.GetProperty("accessToken").GetString()!;

        // 2. Build HTTP client for NBomber
        using var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 3. Scenario: 2 req/s for 30s (scan is heavy ~5s each)
        var scenario = Scenario.Create("scan_endpoint_load", async context =>
        {
            var request = Http.CreateRequest("POST", "/api/scan");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 2,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        var sceneStats = stats.ScenarioStats[0];

        // 4. Print results to console for the demo
        Console.WriteLine($"=== NBomber Results ===");
        Console.WriteLine($"Total OK: {sceneStats.Ok.Request.Count}");
        Console.WriteLine($"Total FAIL: {sceneStats.Fail.Request.Count}");
        Console.WriteLine($"RPS: {sceneStats.Ok.Request.RPS}");
        Console.WriteLine($"P50 latency (ms): {sceneStats.Ok.Latency.Percent50}");
        Console.WriteLine($"P95 latency (ms): {sceneStats.Ok.Latency.Percent95}");
        Console.WriteLine($"Max latency (ms): {sceneStats.Ok.Latency.MaxMs}");

        // 5. Soft assertions for the demo (don't fail the test)
        Assert.True(sceneStats.Ok.Request.Count > 0,
            "Expected at least one successful request");
    }
}