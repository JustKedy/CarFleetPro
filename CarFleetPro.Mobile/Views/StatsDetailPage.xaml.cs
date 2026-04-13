namespace CarFleetPro.Mobile.Views;

public partial class StatsDetailPage : ContentPage
{
    public StatsDetailPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}