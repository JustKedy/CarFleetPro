using CarFleetPro.Mobile.Models;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class VehicleDetailsPage : ContentPage
{
    // Sayfaya hangi aracın detayına bakacağımızı söylüyoruz
    public VehicleDetailsPage(Vehicle selectedVehicle)
    {
        InitializeComponent();
        // Aracın bilgilerini sayfaya bağla
        BindingContext = selectedVehicle; 
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null) await Navigation.PopAsync();
    }

    private async void OnMaintenanceClicked(object? sender, EventArgs e)
    {
        // YUNUS NOT: Bakıma gönderme API entegrasyonu buraya yazılacak.
    }

    private async void OnRentClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PushAsync(new RentalFormPage());
        }
    }
}