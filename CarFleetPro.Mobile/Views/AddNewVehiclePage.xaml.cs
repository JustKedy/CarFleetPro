using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class AddNewVehiclePage : ContentPage
{
    public AddNewVehiclePage()
    {
        InitializeComponent();
    }

    // İPTAL ET butonuna tıklandığında bir önceki sayfaya (Listeye) geri döner
    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}