namespace CarFleetPro.Mobile.Views;

public partial class RentalFormPage : ContentPage
{
    public RentalFormPage()
    {
        InitializeComponent();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        // Bir önceki sayfaya (Detay Sayfası) geri fırlat
        await Navigation.PopAsync();
    }

    // YUNUS NOT: "TAMAMLA" butonu tetiklendiğinde;
    // 1. Kiralama kaydı oluşturulacak (POST /Rentals)
    // 2. Aracın durumu 'Dolu' (StatusId = 2) olarak güncellenecek
    // 3. Başarılı ise ana ekrana yönlendirilecek.
}