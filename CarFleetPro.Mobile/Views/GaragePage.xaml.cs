using CarFleetPro.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class GaragePage : ContentPage
{
    public GaragePage()
    {
        InitializeComponent();
        BindingContext = new GarageViewModel();
    }
}