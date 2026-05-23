using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulseEDR.Desktop.Services;

namespace PulseEDR.Desktop.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private string _scanStatus = "Ready";
    [ObservableProperty] private string _riskScore = "-";
    [ObservableProperty] private string _severity = "-";
    [ObservableProperty] private int _alertCount;
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private string _machineName = "-";

    public ScanViewModel(ApiClient api) => _api = api;

    [RelayCommand]
    private async Task RunScanAsync()
    {
        IsScanning = true;
        ScanStatus = "Scanning...";
        RiskScore = "-";
        Severity = "-";
        AlertCount = 0;

        try
        {
            var doc = await _api.StartScanAsync();
            if (doc == null)
            {
                ScanStatus = "Scan failed.";
                return;
            }

            var root = doc.RootElement;
            MachineName = root.TryGetProperty("machineName", out var mn)
                ? mn.GetString() ?? "-" : "-";

            if (root.TryGetProperty("riskScore", out var rs))
            {
                RiskScore = rs.TryGetProperty("value", out var v)
                    ? v.GetInt32().ToString() : "-";
                Severity = rs.TryGetProperty("severity", out var sv)
                    ? sv.GetString() ?? "-" : "-";
            }

            if (root.TryGetProperty("alerts", out var alerts))
                AlertCount = alerts.GetArrayLength();

            ScanStatus = "Scan complete";
        }
        catch (Exception ex)
        {
            ScanStatus = "Error: " + ex.Message;
        }
        finally { IsScanning = false; }
    }
}