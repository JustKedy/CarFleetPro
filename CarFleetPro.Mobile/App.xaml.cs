using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using CarFleetPro.Mobile.Views; // Görünümleri tanıyabilmesi için

namespace CarFleetPro.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Object Initializer kullanarak hem oluşturuyoruz hem de boyutlarını veriyoruz.
        Window window = new(new NavigationPage(new LoginPage()))
        {
            Width = 400,
            Height = 850
        };

        return window;
    }
}