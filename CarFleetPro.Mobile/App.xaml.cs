using Microsoft.Extensions.DependencyInjection;

namespace CarFleetPro.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // MAUI'nin varsayılan penceresini oluşturuyoruz (AppShell'i çağırarak)
        var window = new Window(new AppShell());

        // Pencere boyutlarını telefon formatına (dikey) sabitliyoruz
        window.Width = 400;
        window.Height = 850;

        return window;
    }
}