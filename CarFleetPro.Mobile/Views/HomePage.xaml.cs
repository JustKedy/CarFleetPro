using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    // DI: MauiProgram singleton olarak kayıt ettiği için her seferinde aynı VM gelir
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel   = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // SAYFA AÇILIŞ ANİMASYONU
        this.Opacity = 0;
        await this.FadeTo(1, 300, Easing.CubicOut);

        // Eski kodun
        await _viewModel.LoadDataAsync();
    }
}