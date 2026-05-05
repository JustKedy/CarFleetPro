using CarFleetPro.Mobile.Services;
using System;
using Microsoft.Maui.Controls;

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

        if (newPwd != confirmPwd)
        {
            await DisplayAlertAsync("Hata", "Yeni şifreler eşleşmiyor!", "Tamam");
            return;
        }

        if (newPwd.Length < 6)
        {
            await DisplayAlertAsync("Hata", "Yeni şifre en az 6 karakter olmalıdır.", "Tamam");
            return;
        }

        var (success, message) = await _apiService.ChangePasswordAsync(oldPwd, newPwd);
        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");

        if (success && Navigation != null)
            await Navigation.PopAsync();
    }
}
