using CommunityToolkit.Mvvm.ComponentModel;
using PulseEDR.Desktop.Services;

namespace PulseEDR.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private int _totalScans;
    [ObservableProperty] private int _totalAlerts;
    [ObservableProperty] private int _criticalAlerts;
    [ObservableProperty] private int _knownVulnerabilities;
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

            if (root.TryGetProperty("totalScans", out var ts))
                TotalScans = ts.GetInt32();

            if (root.TryGetProperty("totalAlerts", out var ta))
                TotalAlerts = ta.GetInt32();

            if (root.TryGetProperty("criticalAlerts", out var ca))
                CriticalAlerts = ca.GetInt32();

            if (root.TryGetProperty("knownVulnerabilities", out var kv))
                KnownVulnerabilities = kv.GetInt32();

            if (root.TryGetProperty("currentRiskScore", out var rs))
            {
                if (rs.TryGetProperty("severity", out var sv))
                    RiskLevel = sv.GetString() ?? "-";
            }

            if (root.TryGetProperty("lastScanAt", out var ls))
                LastScanTime = ls.GetString() ?? "-";
        }
        finally { IsLoading = false; }
    }
}