using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views;

public partial class AdminRegistrationPage : ContentPage
{
    private readonly ApiService _apiService;

    public AdminRegistrationPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PopAsync();
    }

    private async void OnCreateAdminClicked(object? sender, EventArgs e)
    {
        var name = AdminNameEntry.Text?.Trim();
        var email = AdminEmailEntry.Text?.Trim();
        var password = AdminPasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlertAsync("Uyarı", "Ad Soyad, E-Posta ve Şifre alanları zorunludur.", "Tamam");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlertAsync("Hata", "Şifre en az 6 karakter olmalıdır.", "Tamam");
            return;
        }

        var (success, message) = await _apiService.RegisterAdminAsync(name, email, password);
        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");

        if (success && Navigation != null)
            await Navigation.PopAsync();
    }
}