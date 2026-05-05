using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class VehicleDetailsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Vehicle _selectedVehicle;

    
    public VehicleDetailsPage(Vehicle selectedVehicle)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _selectedVehicle = selectedVehicle;
        BindingContext = selectedVehicle; 
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadVehicleDetails();
    }

    private async Task LoadVehicleDetails()
    {
        var detail = await _apiService.GetVehicleDetailsAsync(_selectedVehicle.Id);
        if (detail != null)
        {
            
            StatusBadge.Text = detail.Status;
            StatusBadge.BackgroundColor = detail.Status switch
            {
                "MÃœSAÄ°T" => Color.FromArgb("#10B981"),
                "DOLU" => Color.FromArgb("#EF4444"),
                _ => Color.FromArgb("#F59E0B")
            };

            KmLabel.Text = detail.Mileage.ToString("N0");
            FuelLabel.Text = detail.FuelType;
            GearLabel.Text = detail.TransmissionType;

            
            if (detail.History.Count > 0)
            {
                HistoryList.ItemsSource = detail.History;
            }
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null) await Navigation.PopAsync();
    }

    private async void OnMaintenanceClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync("BakÄ±ma GÃ¶nder", 
            $"{_selectedVehicle.Plaka} plakalÄ± aracÄ± bakÄ±ma gÃ¶ndermek istediÄŸinize emin misiniz?", 
            "Evet", "Ä°ptal");

        if (!confirm) return;

        var (success, message) = await _apiService.SendToMaintenanceAsync(_selectedVehicle.Id);
        await DisplayAlertAsync(success ? "BaÅŸarÄ±lÄ± âœ…" : "Hata âŒ", message, "Tamam");

        if (success && Navigation is not null)
            await Navigation.PopAsync();
    }

    private async void OnRentClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PushAsync(new RentalFormPage(_selectedVehicle));
        }
    }
}
