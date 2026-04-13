using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Services;
using CarFleetPro.Mobile.ViewModels; // HomePage'in istediği modeli bulması için eklendi

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
            // OBSOLETE Uyarısı Çözüldü: DisplayAlert yerine DisplayAlertAsync kullanılıyor
            await DisplayAlertAsync("Eksik Bilgi", "Lütfen E-posta ve şifrenizi girin.", "Tamam");
            return;
        }

        if (LoginButton != null) LoginButton.IsEnabled = false;

        // --- YUNUS'U BEKLEMEDEN BYPASS OPERASYONU ---
        bool success = true;

        if (LoginButton != null) LoginButton.IsEnabled = true;

        if (success)
        {
            if (Navigation is not null)
            {
                try
                {
                    // DI (Dependency Injection) üzerinden sayfayı çağırmayı deniyoruz
                    var homePage = Handler?.MauiContext?.Services.GetService(typeof(HomePage)) as HomePage;

                    if (homePage != null)
                    {
                        await Navigation.PushAsync(homePage);
                    }
                    else
                    {
                        // Eğer servis bulamazsa, HomePage'in istediği ViewModel'i vererek manuel açıyoruz
                        await Navigation.PushAsync(new HomePage(new HomeViewModel(new ApiService())));
                    }
                }
                catch
                {
                    // Herhangi bir çökme durumunda yine manuel başlatma güvencesi (HATA 1 ÇÖZÜMÜ)
                    await Navigation.PushAsync(new HomePage(new HomeViewModel(new ApiService())));
                }
            }
        }
    }

    // YENİ: Şifremi Unuttum Navigasyonu
    private async void OnForgotPasswordTapped(object? sender, EventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PushAsync(new ForgotPasswordPage());
        }
    }
}