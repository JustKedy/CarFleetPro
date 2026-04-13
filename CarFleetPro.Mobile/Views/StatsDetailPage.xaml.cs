using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views;

public partial class StatsDetailPage : ContentPage
{
    private readonly ApiService _apiService;
    private string _filterStatus;

    
    
    
    
    public StatsDetailPage(string filterStatus = "BAKIMDA", string title = "Bakımda Olan Araçlar", int count = 0)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _filterStatus = filterStatus;

        
        HeaderTitle.Text = $"Şu Anda {title}";
        HeaderCount.Text = count.ToString();
    }

    
    public StatsDetailPage() : this("BAKIMDA", "Bakımda Olan Araçlar", 0)
    {
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFilteredVehicles();
    }

    private async Task LoadFilteredVehicles()
    {
        var allVehicles = await _apiService.GetVehiclesAsync();

        var filtered = _filterStatus == "TÜM"
            ? allVehicles
            : allVehicles.Where(v => v.Durum == _filterStatus).ToList();

        StatsList.ItemsSource = filtered;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnStatSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Vehicle vehicle)
        {
            await Navigation.PushAsync(new VehicleDetailsPage(vehicle));
        }
        StatsList.SelectedItem = null;
    }
}