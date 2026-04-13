using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class VehicleDetailsPage : ContentPage
{
    public VehicleDetailsPage()
    {
        InitializeComponent();

        // YUNUS NOT: Buraya ileride dışarıdan seçilen aracın ID'si gelecek
        // BindingContext = new VehicleDetailsViewModel(vehicleId);
    }

    // Geri butonu için motor
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        // Sayfayı yığından (stack) geri fırlatır
        await Navigation.PopAsync();
    }

    // YUNUS İÇİN NOT: Bakım ve Kiralama butonları için Command'ler ViewModel'e taşınacak.
    // Şimdilik tasarımın çalışması için buradalar.
    private async void OnMaintenanceClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Bakım", "Araç bakım moduna alınıyor...", "Tamam");
    }

    private async void OnRentClicked(object? sender, EventArgs e)
    {
        // Kiralama formuna (bir sonraki ekran) uçuş
        // await Navigation.PushAsync(new RentalFormPage());
    }
}