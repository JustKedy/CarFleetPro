using Microsoft.Extensions.Logging;
using CarFleetPro.Mobile.Services;
using CarFleetPro.Mobile.ViewModels;
using CarFleetPro.Mobile.Views;

namespace CarFleetPro.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ─── Servisler ────────────────────────────────────────────────
            // ApiService tek paylaşımlı olsun — aynı HTTP client yeniden kullanılır
            builder.Services.AddSingleton<ApiService>();

            // ─── ViewModels ───────────────────────────────────────────────
            // Singleton: liste sayfalarına geri dönüldüğünde veriler sıfırdan yüklenmez
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<GarageViewModel>();
            builder.Services.AddSingleton<FleetManagementViewModel>();

            // ─── Sayfalar (Pages) ─────────────────────────────────────────
            // DİKKAT: Sayfaların AddSingleton olması MAUI'de Navigation.PushAsync kullanımında çökmeye neden olur (Page already has a parent hatası).
            // ÇÖZÜM: MAUI'de sayfalar Transient (her navigasyonda yeni) olmalıdır. 
            // Verinin kaybolmamasını sağlayan şey yukarıdaki ViewModellerin Singleton olmasıdır! 
            // Yeni sayfa açıldığında hazır ViewModel'i alır ve liste sıfırdan "yüklenmez".
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<GaragePage>();
            builder.Services.AddTransient<FleetManagementPage>();

            // Transient: form sayfası her açılışta temiz gelmeli
            builder.Services.AddTransient<AddNewVehiclePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
    }
}
