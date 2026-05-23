using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulseEDR.Desktop.Services;

namespace PulseEDR.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ApiClient Api { get; } = new ApiClient();

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _statusText = "Not connected";

    [ObservableProperty]
    private bool _isLoggedIn = false;

    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = "admin";

    public DashboardViewModel DashboardVM { get; }
    public ScanViewModel ScanVM { get; }
    public AlertsViewModel AlertsVM { get; }

    public MainViewModel()
    {
        DashboardVM = new DashboardViewModel(Api);
        ScanVM = new ScanViewModel(Api);
        AlertsVM = new AlertsViewModel(Api);
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        StatusText = "Connecting...";
        var token = await Api.LoginAsync(Username, Password);
        if (token != null)
        {
            IsLoggedIn = true;
            StatusText = "Connected as " + Username;
            CurrentView = DashboardVM;
            await DashboardVM.LoadAsync();
        }
        else
        {
            StatusText = "Login failed. Check credentials.";
        }
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentView = DashboardVM;
        _ = DashboardVM.LoadAsync();
    }

    [RelayCommand]
    private void NavigateToScan()
    {
        CurrentView = ScanVM;
    }

    [RelayCommand]
    private void NavigateToAlerts()
    {
        CurrentView = AlertsVM;
        _ = AlertsVM.LoadAsync();
    }
}