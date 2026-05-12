using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Storage;

namespace CarFleetPro.Mobile.Views;

public partial class SettingsPage : ContentPage
{
    private readonly ApiService _apiService;

    private bool _maintenanceNotificationsEnabled = true;
    private bool _rentalNotificationsEnabled = true;
    private bool _availabilityNotificationsEnabled;

    public SettingsPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        UpdateThemeUi();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateThemeUi();
        await LoadProfileCard();
    }

    // ── Profil kartını API'den doldur ────────────────────────────────────────
    private async Task LoadProfileCard()
    {
        try
        {
            var profile = await _apiService.GetProfileAsync();
            if (profile != null)
            {
                ProfileNameLabel.Text = profile.FullName;
                ProfileRoleLabel.Text = profile.Role switch
                {
                    "Yönetici" => "Yetki: Sistem Yöneticisi",
                    "Çalışan"  => "Yetki: Çalışan",
                    _          => $"Yetki: {profile.Role}"
                };

                AdminPanelBorder.IsVisible = profile.Role == "Yönetici";
            }
        }
        catch
        {
            ProfileNameLabel.Text = "—";
        }
    }

    // ── Tema ─────────────────────────────────────────────────────────────────
    private void UpdateThemeUi()
    {
        var isDark = ThemeService.IsDark;
        var suffix = isDark ? "white" : "black";

        ThemeModeLabel.Text = ThemeService.CurrentThemeName;
        ThemeIcon.Source = isDark ? "moon_white.svg" : "sun_black.svg";
        ThemeChevronIcon.Source = $"arrow_right_{suffix}.svg";
        ProfileIcon.Source = $"user_{suffix}.svg";
        PasswordIcon.Source = $"lock_{suffix}.svg";
        ProfileArrowIcon.Source = $"arrow_right_{suffix}.svg";
        PasswordArrowIcon.Source = $"arrow_right_{suffix}.svg";

        MaintenanceNotifIcon.Source = GetBellIcon(_maintenanceNotificationsEnabled, isDark);
        RentalNotifIcon.Source = GetBellIcon(_rentalNotificationsEnabled, isDark);
        AvailableNotifIcon.Source = GetBellIcon(_availabilityNotificationsEnabled, isDark);
    }

    private static string GetBellIcon(bool enabled, bool isDark)
    {
        var state = enabled ? "on" : "off";
        var suffix = isDark ? "white" : "black";
        return $"bell_{state}_{suffix}.svg";
    }

    private void OnThemeTapped(object? sender, EventArgs e)
    {
        ThemeService.Toggle();
        UpdateThemeUi();
    }

    // ── Navigasyon ───────────────────────────────────────────────────────────
    private async void OnAdminMenuTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminMenuPage());
    }

    private async void OnProfileTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PushAsync(new ProfileSettingsPage());
    }

    private async void OnChangePasswordTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PushAsync(new ChangePasswordPage());
    }

    // ── Çıkış Yap ────────────────────────────────────────────────────────────
    private async void OnLogoutTapped(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Çıkış Yap",
            "Hesabınızdan çıkış yapmak istediğinize emin misiniz?",
            "Evet, Çık",
            "İptal");

        if (!confirm) return;

        // JWT token'ı temizle
        SecureStorage.Default.Remove("jwt_token");

        // LoginPage'e root olarak geç (geri dönüş olmasın)
        if (Application.Current?.Windows.Count > 0)
        {
            var window = Application.Current.Windows[0];
            var loginPage = new NavigationPage(new LoginPage());
            window.Page = loginPage;
        }
    }

    // ── Bildirim toggleları ──────────────────────────────────────────────────
    private void ToggleNotificationIcon(Image icon, ref bool enabled)
    {
        enabled = !enabled;
        icon.Source = GetBellIcon(enabled, ThemeService.IsDark);
    }

    private void OnMaintenanceNotifTapped(object? sender, EventArgs e)
    {
        ToggleNotificationIcon(MaintenanceNotifIcon, ref _maintenanceNotificationsEnabled);
    }

    private void OnRentalNotifTapped(object? sender, EventArgs e)
    {
        ToggleNotificationIcon(RentalNotifIcon, ref _rentalNotificationsEnabled);
    }

    private void OnAvailableNotifTapped(object? sender, EventArgs e)
    {
        ToggleNotificationIcon(AvailableNotifIcon, ref _availabilityNotificationsEnabled);
    }
}
