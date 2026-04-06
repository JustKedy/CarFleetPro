using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Eksik Bilgi", "Lütfen E-posta ve şifrenizi girin.", "Tamam");
            return;
        }

        if (LoginButton != null) LoginButton.IsEnabled = false;

        var (success, message) = await _apiService.LoginAsync(email, password);

        if (LoginButton != null) LoginButton.IsEnabled = true;

        if (success)
        {
            // Başarılı olursa anasayfaya yönlendir
            await Navigation.PushAsync(new HomePage());
        }
        else
        {
            await DisplayAlert("Giriş Başarısız", message, "Tamam");
        }
    }
}