namespace CarFleetPro.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnOpenFormClicked(object sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(new FleetManagementPage());
    }
}