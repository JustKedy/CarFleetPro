using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using CarFleetPro.Mobile.Views;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        // Kaydedilmiş temayı başlangıçta uygula
        ThemeService.LoadSavedTheme();
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