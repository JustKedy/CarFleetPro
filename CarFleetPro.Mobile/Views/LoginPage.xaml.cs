using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Services;
using CarFleetPro.Mobile.ViewModels;

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
            await DisplayAlertAsync("Eksik Bilgi", "Lütfen E-posta ve şifrenizi girin.", "Tamam");
            return;
        }

        if (LoginButton != null) LoginButton.IsEnabled = false;

        var (success, message) = await _apiService.LoginAsync(email, password);

        if (LoginButton != null) LoginButton.IsEnabled = true;

        if (success)
        {
            try
            {
                
                var homePage = Handler?.MauiContext?.Services.GetService<HomePage>();
                
                if (homePage == null)
                {
                    
                    var api = Handler?.MauiContext?.Services.GetService<ApiService>() ?? new ApiService();
                    homePage = new HomePage(new HomeViewModel(api));
                }

                await Navigation.PushAsync(homePage);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("UI/Navigasyon Hatası", $"HomePage açılırken hata oluştu: {ex.Message}", "Tamam");
            }
        }
        else
        {
            await DisplayAlertAsync("Giriş Başarısız", message, "Tamam");
        }
    }

    
    private async void OnForgotPasswordTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PushAsync(new ForgotPasswordPage());
        }
    }
}