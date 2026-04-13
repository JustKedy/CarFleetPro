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
        // Sayfa her açıldığında (Bottom Nav bar geçişleri veya form dönüşleri dahil) 
        // verileri yenile. API tarafı eğer veri değişmemişse anında cache döner, ekran titremez.
        await _viewModel.LoadDataAsync();
    }
}