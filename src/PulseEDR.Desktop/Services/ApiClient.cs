using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PulseEDR.Desktop.Services;

/// <summary>
/// Thin wrapper around HttpClient that handles auth token and
/// serializes/deserializes JSON responses from the PulseEDR API.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string? Token { get; private set; }
    public bool IsAuthenticated => Token != null;

    public ApiClient()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5015")
        };
    }

    public void SetToken(string token)
    {
        Token = token;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        Token = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    // AUTH
public async Task<string?> LoginAsync(string username, string password)
{
    var response = await _http.PostAsJsonAsync("/api/auth/login",
        new { username, password });

    if (!response.IsSuccessStatusCode) return null;

    var body = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(body);
    var root = doc.RootElement;

    // Try common token property names
    string? token = null;
    if (root.TryGetProperty("token", out var t)) token = t.GetString();
    else if (root.TryGetProperty("accessToken", out var at)) token = at.GetString();
    else if (root.TryGetProperty("jwt", out var j)) token = j.GetString();

    if (token != null) SetToken(token);
    return token;
}

    // SCAN
    public async Task<JsonDocument?> StartScanAsync()
    {
        var response = await _http.PostAsync("/api/scan", null);
        if (!response.IsSuccessStatusCode) return null;
        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
    }

    public async Task<JsonDocument?> GetScanAsync(string scanId)
    {
        var response = await _http.GetAsync("/api/scan/" + scanId);
        if (!response.IsSuccessStatusCode) return null;
        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
    }

    // DASHBOARD
    public async Task<JsonDocument?> GetDashboardAsync()
    {
        var response = await _http.GetAsync("/api/dashboard");
        if (!response.IsSuccessStatusCode) return null;
        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
    }

    // ALERTS
    public async Task<JsonDocument?> GetAlertsAsync(int page = 1, int pageSize = 20)
    {
        var response = await _http.GetAsync(
            $"/api/alerts?page={page}&pageSize={pageSize}");
        if (!response.IsSuccessStatusCode) return null;
        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
    }

    // CVE
    public async Task<JsonDocument?> GetCvesAsync()
    {
        var response = await _http.GetAsync("/api/cve");
        if (!response.IsSuccessStatusCode) return null;
        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
    }
}