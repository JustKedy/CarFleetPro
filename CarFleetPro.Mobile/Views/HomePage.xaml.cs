using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage()
    {
        InitializeComponent();

        _viewModel = new HomeViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Araç eklendiğinde ana sayfa istatistiklerini de yenile
        WeakReferenceMessenger.Default.Register<VehicleAddedMessage>(this, async (recipient, message) =>
        {
            await _viewModel.LoadDataAsync();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Sayfa arkada çalıştığında bellek sızıntısını önle
        WeakReferenceMessenger.Default.Unregister<VehicleAddedMessage>(this);
    }
}