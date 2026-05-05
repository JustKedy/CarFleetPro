using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class AlertsPage : ContentPage
{
    private readonly ApiService _apiService;

    public AlertsPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAlerts();
    }

    private async Task LoadAlerts()
    {
        var alerts = await _apiService.GetAlertsAsync();
        AlertsList.ItemsSource = alerts;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PopAsync();
    }

    private async void OnAlertSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AlertInfo alert)
        {
            await DisplayAlertAsync(alert.AlertType, $"{alert.Title}\n{alert.Subtitle}\n{alert.Detail}", "Tamam");
        }
        AlertsList.SelectedItem = null;
    }
}
