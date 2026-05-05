using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views;

public partial class SettingsPage : ContentPage
{
    private bool _maintenanceNotificationsEnabled = true;
    private bool _rentalNotificationsEnabled = true;
    private bool _availabilityNotificationsEnabled;

    public SettingsPage()
    {
        InitializeComponent();
        UpdateThemeUi();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateThemeUi();
    }

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
        InvoiceArrowIcon.Source = $"arrow_right_{suffix}.svg";
        StaffArrowIcon.Source = $"arrow_right_{suffix}.svg";

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

    private async void OnProfileTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new ProfileSettingsPage());
        }
    }

    private async void OnChangePasswordTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new ChangePasswordPage());
        }
    }

    private async void OnAddAdminTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new StaffManagementPage());
        }
    }

    private async void OnInvoiceTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new InvoicePage());
        }
    }

    private async void OnManageNotificationsClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new NotificationManagementPage());
        }
    }

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
