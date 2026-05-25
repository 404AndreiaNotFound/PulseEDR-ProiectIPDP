using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PulseEDR.IntegrationTests;

public class ApiEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndOk()
    {
        var payload = new { username = "admin", password = "admin" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var payload = new { username = "admin", password = "wrong_password" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetScan_WithoutAuth_Returns401()
    {
        var fakeId = Guid.NewGuid();

        var response = await _client.GetAsync("/api/scan/" + fakeId);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}