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
        [cite_start] BindingContext = selectedVehicle; [cite: 3]
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        [cite_start] if (Navigation is not null) await Navigation.PopAsync(); [cite: 3]
    }
}

    private async void OnMaintenanceClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
        {
            // DisplayAlert yerine DisplayAlertAsync kullanıldı!
            await Shell.Current.DisplayAlertAsync("Bakım", "Araç bakım moduna alınıyor...", "Tamam");
        }
    }

    private async void OnRentClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PushAsync(new RentalFormPage());
        }
    }
}