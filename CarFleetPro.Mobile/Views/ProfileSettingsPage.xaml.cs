namespace CarFleetPro.Mobile.Views;

public partial class ProfileSettingsPage : ContentPage
{
    public ProfileSettingsPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnChangePasswordTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new ChangePasswordPage());
        }
    }

    private async void OnUpdateProfileClicked(object? sender, EventArgs e)
    {
        // YUNUS NOT: Profil güncelleme API entegrasyonu buraya yazılacak.
    }
}
