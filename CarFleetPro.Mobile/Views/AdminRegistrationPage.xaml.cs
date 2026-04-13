namespace CarFleetPro.Mobile.Views;

public partial class AdminRegistrationPage : ContentPage
{
    public AdminRegistrationPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}