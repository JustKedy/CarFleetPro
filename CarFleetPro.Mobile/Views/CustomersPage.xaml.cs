namespace CarFleetPro.Mobile.Views;

public partial class CustomersPage : ContentPage
{
    public CustomersPage()
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

    private async void OnCustomerSelected(object? sender, SelectionChangedEventArgs e)
    {
        // YUNUS NOT: Müşteri bilgisi çekme entegrasyonu buraya yazılacak.
    }

    // YUNUS NOT: Bir müşteriye tıklandığında;
    // O müşterinin kiralama geçmişini ve borç durumunu göreceğimiz
    // 'CustomerDetailsPage' ekranına ID taşınacak.
}