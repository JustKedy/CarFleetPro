using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ApiService _apiService;
    private string? _resetToken;
    private string? _userEmail;

    public ForgotPasswordPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnBackToLoginTapped(object? sender, EventArgs e)
    {
        if (Navigation is not null)
            await Navigation.PopAsync();
    }

    private async void OnSendCodeClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlertAsync("Uyarı", "Lütfen e-posta adresinizi girin.", "Tamam");
            return;
        }

        _userEmail = email;
        var (success, message, token) = await _apiService.ForgotPasswordAsync(email);
        _resetToken = token;
        
        await DisplayAlertAsync("Bilgi", message, "Tamam");

        if (success && _resetToken != null)
        {
            // Token alındı — kullanıcıya yeni şifre sormak için prompt göster
            var newPassword = await DisplayPromptAsync(
                "Yeni Şifre", 
                "E-postanıza gönderilen kodu onayladıktan sonra yeni şifrenizi belirleyin:",
                "Sıfırla", "İptal",
                placeholder: "Yeni şifrenizi girin",
                maxLength: 50);

            if (!string.IsNullOrEmpty(newPassword))
            {
                var (resetSuccess, resetMessage) = await _apiService.ResetPasswordAsync(_userEmail, _resetToken, newPassword);
                await DisplayAlertAsync(resetSuccess ? "Başarılı ✅" : "Hata ❌", resetMessage, "Tamam");

                if (resetSuccess && Navigation is not null)
                    await Navigation.PopAsync();
            }
        }
    }
}