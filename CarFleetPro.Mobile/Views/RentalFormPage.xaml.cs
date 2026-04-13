namespace CarFleetPro.Mobile.Views;

public partial class RentalFormPage : ContentPage
{
    public RentalFormPage()
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

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        // Bir önceki sayfaya (Detay Sayfası) geri fırlat
        await Navigation.PopAsync();
    }

    private async void OnRentCompleteClicked(object? sender, EventArgs e)
    {
        // YUNUS NOT: Kiralama tamamlama işlemi API entegrasyonu buraya gerçekleştirilecek.
    }

    // YUNUS NOT: "TAMAMLA" butonu tetiklendiğinde;
    // 1. Kiralama kaydı oluşturulacak (POST /Rentals)
    // 2. Aracın durumu 'Dolu' (StatusId = 2) olarak güncellenecek
    // 3. Başarılı ise ana ekrana yönlendirilecek.
}