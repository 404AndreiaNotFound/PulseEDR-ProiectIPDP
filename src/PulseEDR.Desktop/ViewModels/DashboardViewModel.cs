using CommunityToolkit.Mvvm.ComponentModel;
using PulseEDR.Desktop.Services;

namespace PulseEDR.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private int _totalScans;
    [ObservableProperty] private int _totalAlerts;
    [ObservableProperty] private string _riskLevel = "-";
    [ObservableProperty] private string _lastScanTime = "-";
    [ObservableProperty] private bool _isLoading;

    public DashboardViewModel(ApiClient api) => _api = api;

public async Task LoadAsync()
{
    IsLoading = true;
    try
    {
        var doc = await _api.GetDashboardAsync();
        if (doc == null) return;

        var root = doc.RootElement;

        // Try multiple possible property names
        if (root.TryGetProperty("totalScans", out var ts)) TotalScans = ts.GetInt32();
        else if (root.TryGetProperty("scanCount", out var sc)) TotalScans = sc.GetInt32();

        if (root.TryGetProperty("totalAlerts", out var ta)) TotalAlerts = ta.GetInt32();
        else if (root.TryGetProperty("alertCount", out var ac)) TotalAlerts = ac.GetInt32();

        if (root.TryGetProperty("overallRiskLevel", out var rl)) RiskLevel = rl.GetString() ?? "-";
        else if (root.TryGetProperty("riskLevel", out var rl2)) RiskLevel = rl2.GetString() ?? "-";
        else if (root.TryGetProperty("severity", out var sv)) RiskLevel = sv.GetString() ?? "-";

        if (root.TryGetProperty("lastScanAt", out var ls)) LastScanTime = ls.GetString() ?? "-";
        else if (root.TryGetProperty("lastScan", out var ls2)) LastScanTime = ls2.GetString() ?? "-";
    }
    finally { IsLoading = false; }
}
}