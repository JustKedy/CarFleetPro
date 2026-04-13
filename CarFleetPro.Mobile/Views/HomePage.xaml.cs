using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel   = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        
        this.Opacity = 0;
        await this.FadeToAsync(1, 300, Easing.CubicOut);

        
        await _viewModel.LoadDataAsync();
    }
}