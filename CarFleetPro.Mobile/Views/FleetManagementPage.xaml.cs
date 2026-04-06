using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class FleetManagementPage : ContentPage
{
    public FleetManagementPage()
    {
        InitializeComponent();
    }

    // YENİ ARAÇ EKLE butonuna tıklandığında çalışır
    private async void OnAddNewVehicleClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddNewVehiclePage());
    }
}