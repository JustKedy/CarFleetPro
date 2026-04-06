using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class GaragePage : ContentPage
{
    private readonly GarageViewModel _viewModel;

    public GaragePage()
    {
        InitializeComponent();
        _viewModel = new GarageViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Araç eklendiğinde Garaj sayfasını da yenilemek için dinleyici ekliyoruz
        WeakReferenceMessenger.Default.Register<VehicleAddedMessage>(this, async (recipient, message) =>
        {
            await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Sayfa arkada çalıştığında gereksiz bellek harcamaması için iptal et
        WeakReferenceMessenger.Default.Unregister<VehicleAddedMessage>(this);
    }
}