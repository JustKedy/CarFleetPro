using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Services;

/// <summary>
/// Uygulama genelinde açık/karanlık tema yönetimi.
/// Tercih Preferences'a kaydedilir, uygulama yeniden başladığında hatırlanır.
/// </summary>
public static class ThemeService
{
    private const string ThemeKey = "app_theme";

    public static bool IsDark => Application.Current?.UserAppTheme == AppTheme.Dark;

    public static string CurrentThemeName => IsDark ? "Karanlık Tema" : "Açık Tema";

    /// <summary>
    /// Uygulama başlarken kaydedilmiş temayı yükler.
    /// App.xaml.cs içinden OnStart'ta çağrılmalı.
    /// </summary>
    public static void LoadSavedTheme()
    {
        var saved = Preferences.Get(ThemeKey, "Light");
        Apply(saved == "Dark");
    }

    /// <summary>
    /// Temayı değiştirir ve Preferences'a kaydeder.
    /// </summary>
    public static void Toggle()
    {
        Apply(!IsDark);
    }

    public static void Apply(bool dark)
    {
        if (Application.Current == null) return;

        Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
        Preferences.Set(ThemeKey, dark ? "Dark" : "Light");
    }
}
