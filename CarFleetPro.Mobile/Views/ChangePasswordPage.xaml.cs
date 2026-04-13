namespace CarFleetPro.Mobile.Views;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage()
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

    private async void OnChangePasswordClicked(object? sender, EventArgs e)
    {
        // YUNUS NOT: Şifre değiştirme API entegrasyonu buraya yazılacak.
    }
}
