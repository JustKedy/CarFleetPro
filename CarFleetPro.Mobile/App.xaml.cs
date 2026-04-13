using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using CarFleetPro.Mobile.Views; 

namespace CarFleetPro.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        
        Window window = new(new NavigationPage(new LoginPage()))
        {
            Width = 400,
            Height = 850
        };

        return window;
    }
}