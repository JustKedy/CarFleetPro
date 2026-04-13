using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views;

public partial class GaragePage : ContentPage
{
    private readonly GarageViewModel _viewModel;

    // DI: Singleton VM ile sayfa state'ini korur
    public GaragePage(GarageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel     = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa her ekrana geldiğinde yenileme komutunu çalıştır
        await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
    }
}