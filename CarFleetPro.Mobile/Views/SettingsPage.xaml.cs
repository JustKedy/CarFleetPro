namespace CarFleetPro.Mobile.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var fullName = await Services.SessionManager.GetFullNameAsync();
        var role = await Services.SessionManager.GetRoleAsync();

        ProfileNameLabel.Text = string.IsNullOrEmpty(fullName) ? "Kullanıcı" : fullName;
        ProfileRoleLabel.Text = $"Yetki: {role}";
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

    private void ToggleNotificationIcon(Image icon)
    {
        var currentSource = icon.Source as FileImageSource;
        if (currentSource != null)
        {
            if (currentSource.File == "bell_on_black.svg")
            {
                icon.Source = "bell_off_black.svg";
            }
            else
            {
                icon.Source = "bell_on_black.svg";
            }
        }
    }

    private void OnMaintenanceNotifTapped(object? sender, EventArgs e)
    {
        ToggleNotificationIcon(MaintenanceNotifIcon);
    }

    private void OnRentalNotifTapped(object? sender, EventArgs e)
    {
        ToggleNotificationIcon(RentalNotifIcon);
    }

    private void OnAvailableNotifTapped(object? sender, EventArgs e)
    {
        ToggleNotificationIcon(AvailableNotifIcon);
    }
}