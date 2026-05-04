using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views;

public partial class ChangePasswordPage : ContentPage
{
    private readonly ApiService _apiService;

    public ChangePasswordPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PopAsync();
    }

    private async void OnChangePasswordClicked(object? sender, EventArgs e)
    {
        var oldPwd = OldPasswordEntry.Text?.Trim();
        var newPwd = NewPasswordEntry.Text?.Trim();
        var confirmPwd = ConfirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirmPwd))
        {
            await DisplayAlertAsync("Uyarı", "Tüm alanları doldurunuz.", "Tamam");
            return;
        }

        if (NewPasswordEntry.Text != confirmPwd)
        {
            await DisplayAlertAsync("Hata", "Yeni şifreler eşleşmiyor!", "Tamam");
            return;
        }

        if (NewPasswordEntry.Text?.Length < 6)
        {
            await DisplayAlertAsync("Hata", "Yeni şifre en az 6 karakter olmalıdır.", "Tamam");
            return;
        }

        var (success, message) = await _apiService.ChangePasswordAsync(oldPwd, NewPasswordEntry.Text!);
        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");

        if (success && Navigation != null)
            await Navigation.PopAsync();
    }

    private void OnNewPasswordTextChanged(object? sender, TextChangedEventArgs e)
    {
        var pwd = e.NewTextValue ?? string.Empty;

        // Reset
        Strength1.BackgroundColor = Color.FromArgb("#E5E7EB");
        Strength2.BackgroundColor = Color.FromArgb("#E5E7EB");
        Strength3.BackgroundColor = Color.FromArgb("#E5E7EB");
        StrengthLabel.Text = "Şifre gücü bekleniyor...";
        StrengthLabel.TextColor = Color.FromArgb("#6B7280");

        if (string.IsNullOrEmpty(pwd)) return;

        int score = 0;
        if (pwd.Length >= 6) score++;
        if (System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[A-Z]") && 
            System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[0-9]")) score++;
        if (System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[^a-zA-Z0-9]")) score++;

        if (score == 1 || (score == 0 && pwd.Length > 0))
        {
            Strength1.BackgroundColor = Color.FromArgb("#EF4444"); // Kırmızı
            StrengthLabel.Text = "Zayıf";
            StrengthLabel.TextColor = Color.FromArgb("#EF4444");
        }
        else if (score == 2)
        {
            Strength1.BackgroundColor = Color.FromArgb("#F59E0B"); // Sarı
            Strength2.BackgroundColor = Color.FromArgb("#F59E0B");
            StrengthLabel.Text = "Orta";
            StrengthLabel.TextColor = Color.FromArgb("#F59E0B");
        }
        else if (score >= 3)
        {
            Strength1.BackgroundColor = Color.FromArgb("#10B981"); // Yeşil
            Strength2.BackgroundColor = Color.FromArgb("#10B981");
            Strength3.BackgroundColor = Color.FromArgb("#10B981");
            StrengthLabel.Text = "Güçlü";
            StrengthLabel.TextColor = Color.FromArgb("#10B981");
        }
    }
}
