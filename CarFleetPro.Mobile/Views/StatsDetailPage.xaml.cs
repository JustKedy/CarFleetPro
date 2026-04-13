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

    private async void OnStatSelected(object? sender, SelectionChangedEventArgs e)
    {
        // YUNUS NOT: Seçilen istatistik detaylarına gitme entegrasyonu buraya yazılacak.
    }
}