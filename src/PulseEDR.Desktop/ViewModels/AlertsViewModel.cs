using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PulseEDR.Desktop.Services;

namespace PulseEDR.Desktop.ViewModels;

public record AlertRow(string Title, string Severity, string Category, string Score);

public partial class AlertsViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private bool _isLoading;
    public ObservableCollection<AlertRow> Alerts { get; } = new();

    public AlertsViewModel(ApiClient api) => _api = api;

    public async Task LoadAsync()
    {
        IsLoading = true;
        Alerts.Clear();
        try
        {
            var doc = await _api.GetAlertsAsync();
            if (doc == null) return;

            var root = doc.RootElement;
            var items = root.TryGetProperty("items", out var arr) ? arr : root;

            foreach (var item in items.EnumerateArray())
            {
                Alerts.Add(new AlertRow(
                    item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                    item.TryGetProperty("severity", out var s) ? s.GetString() ?? "" : "",
                    item.TryGetProperty("category", out var c) ? c.GetString() ?? "" : "",
                    item.TryGetProperty("score", out var sc) ? sc.GetInt32().ToString() : "0"
                ));
            }
        }
        finally { IsLoading = false; }
    }
}