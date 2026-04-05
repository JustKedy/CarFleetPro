using CarFleetPro.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();

        // Tasarým ile Veriyi Birleþtiren Köprü!
        BindingContext = new HomeViewModel();
    }
}