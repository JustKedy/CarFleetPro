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

            
            
            builder.Services.AddSingleton<ApiService>();

            
            
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<GarageViewModel>();
            builder.Services.AddSingleton<FleetManagementViewModel>();

            
            
            
            
            
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<GaragePage>();
            builder.Services.AddTransient<FleetManagementPage>();

            
            builder.Services.AddTransient<AddNewVehiclePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
    }
}
