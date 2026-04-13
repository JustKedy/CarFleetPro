using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class VehicleDetailsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Vehicle _selectedVehicle;

    // Sayfaya hangi aracın detayına bakacağımızı söylüyoruz
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
            // UI alanlarını güncelle
            StatusBadge.Text = detail.Status;
            StatusBadge.BackgroundColor = detail.Status switch
            {
                "MÜSAİT" => Color.FromArgb("#10B981"),
                "DOLU" => Color.FromArgb("#EF4444"),
                _ => Color.FromArgb("#F59E0B")
            };

            KmLabel.Text = detail.Mileage.ToString("N0");
            FuelLabel.Text = detail.FuelType;
            GearLabel.Text = detail.TransmissionType;

            // Geçmiş timeline
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
        var confirm = await DisplayAlertAsync("Bakıma Gönder", 
            $"{_selectedVehicle.Plaka} plakalı aracı bakıma göndermek istediğinize emin misiniz?", 
            "Evet", "İptal");

        if (!confirm) return;

        var (success, message) = await _apiService.SendToMaintenanceAsync(_selectedVehicle.Id);
        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");

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