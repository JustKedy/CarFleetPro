namespace CarFleetPro.Mobile.Views;

public partial class AlertsPage : ContentPage
{
    public AlertsPage()
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

    private async void OnAlertSelected(object? sender, SelectionChangedEventArgs e)
    {
        // YUNUS NOT: Seçilen bildirim detayı açılışı API entegrasyonu buraya yazılacak.
    }

    // YUNUS NOT: Bir bildirime tıklandığında;
    // Eğer kiralama gecikmesiyse 'RentalDetails' sayfasına,
    // Eğer bakım uyarısıysa 'VehicleDetails' sayfasına yönlendirme yapılacak.
}