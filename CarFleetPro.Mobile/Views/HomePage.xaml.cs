using CarFleetPro.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();

        // Yeni Dashboard sayfamızın motorunu bağlıyoruz
        BindingContext = new HomeViewModel();
    }
}