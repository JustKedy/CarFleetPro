using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ApiService _apiService;
    private string? _userEmail;
    private int _currentStep = 1;

    public ForgotPasswordPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    // ── Adım geçişleri ───────────────────────────────────────────────────────
    private void GoToStep(int step)
    {
        _currentStep = step;

        Step1Panel.IsVisible = step == 1;
        Step2Panel.IsVisible = step == 2;
        Step3Panel.IsVisible = step == 3;

        PageTitle.Text = step switch
        {
            1 => "Şifrenizi mi Unuttunuz?",
            2 => "Doğrulama Kodu",
            3 => "Yeni Şifre Belirle",
            _ => "Şifre Sıfırla"
        };

        PageSubtitle.Text = step switch
        {
            1 => "Kayıtlı e-posta adresinizi girin, 6 haneli doğrulama kodu gönderelim.",
            2 => $"'{_userEmail}' adresine gönderilen 6 haneli kodu giriniz.",
            3 => "Yeni şifrenizi belirleyin. En az 6 karakter kullanınız.",
            _ => ""
        };

        // Adım indikatör renklerini güncelle
        UpdateStepIndicators(step);
    }

    private void UpdateStepIndicators(int activeStep)
    {
        StepIndicator1.TextColor = activeStep >= 1 ? Colors.White : Color.FromArgb("#9CA3AF");
        StepIndicator2.TextColor = activeStep >= 2 ? Colors.White : Color.FromArgb("#9CA3AF");
        StepIndicator3.TextColor = activeStep >= 3 ? Colors.White : Color.FromArgb("#9CA3AF");
    }

    // ── ADIM 1: Kod gönder ───────────────────────────────────────────────────
    private async void OnSendCodeClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlertAsync("Uyarı", "Lütfen e-posta adresinizi girin.", "Tamam");
            return;
        }

        SendCodeButton.IsEnabled = false;
        SendCodeButton.Text = "Gönderiliyor...";

        _userEmail = email;
        var (success, message) = await _apiService.ForgotPasswordAsync(email);

        SendCodeButton.IsEnabled = true;
        SendCodeButton.Text = "DOĞRULAMA KODU GÖNDER";

        if (success)
        {
            GoToStep(2);
        }
        else
        {
            await DisplayAlertAsync("Hata", message, "Tamam");
        }
    }

    // ── ADIM 2: Kodu doğrula ─────────────────────────────────────────────────
    private void OnVerifyCodeClicked(object? sender, EventArgs e)
    {
        var code = OtpEntry.Text?.Trim();
        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            _ = DisplayAlertAsync("Uyarı", "Lütfen 6 haneli kodu eksiksiz girin.", "Tamam");
            return;
        }
        // Kodu saklıyoruz, Adım 3'te kullanılacak
        GoToStep(3);
    }

    // ── Yeniden kod gönder ───────────────────────────────────────────────────
    private async void OnResendCodeTapped(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_userEmail)) return;

        var (success, _) = await _apiService.ForgotPasswordAsync(_userEmail);
        if (success)
            await DisplayAlertAsync("Gönderildi ✅", "Yeni doğrulama kodu e-posta adresinize gönderildi.", "Tamam");
        else
            await DisplayAlertAsync("Hata", "Kod gönderilemedi. Lütfen tekrar deneyin.", "Tamam");
    }

    // ── ADIM 3: Şifreyi sıfırla ──────────────────────────────────────────────
    private async void OnResetPasswordClicked(object? sender, EventArgs e)
    {
        var otp      = OtpEntry.Text?.Trim();
        var newPwd   = NewPasswordEntry.Text;
        var confirm  = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirm))
        {
            await DisplayAlertAsync("Uyarı", "Tüm alanları doldurunuz.", "Tamam");
            return;
        }

        if (newPwd != confirm)
        {
            await DisplayAlertAsync("Hata", "Şifreler eşleşmiyor!", "Tamam");
            return;
        }

        if (newPwd.Length < 6)
        {
            await DisplayAlertAsync("Hata", "Şifre en az 6 karakter olmalıdır.", "Tamam");
            return;
        }

        // OTP'yi Token alanı olarak API'ye gönder (API cache'deki ile karşılaştırıyor)
        var (success, message) = await _apiService.ResetPasswordAsync(_userEmail!, otp!, newPwd);

        if (success)
        {
            await DisplayAlertAsync("Başarılı ✅", "Şifreniz sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.", "Giriş Yap");
            if (Navigation is not null)
                await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Hata ❌", message, "Tamam");
            // Hatalı kodsa adım 2'ye geri dön
            GoToStep(2);
        }
    }

    // ── Geri ─────────────────────────────────────────────────────────────────
    private async void OnBackToLoginTapped(object? sender, EventArgs e)
    {
        if (_currentStep > 1)
        {
            GoToStep(_currentStep - 1);
            return;
        }

        if (Navigation is not null)
            await Navigation.PopAsync();
    }
}
