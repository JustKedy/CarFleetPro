using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class VehicleDetailsPage : ContentPage
{
    public VehicleDetailsPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        // "Null check can be simplified" uyarısı için modern C# kullanımı:
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
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