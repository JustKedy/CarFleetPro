using CarFleetPro.Mobile.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class ProfileSettingsPage : ContentPage
{
    private readonly ApiService _apiService;

    public ProfileSettingsPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfile();
    }

    private async Task LoadProfile()
    {
        var profile = await _apiService.GetProfileAsync();
        if (profile != null)
        {
            NameEntry.Text = profile.FullName;
            EmailEntry.Text = profile.Email;
            PhoneEntry.Text = profile.PhoneNumber ?? "";
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PopAsync();
    }

    private async void OnChangePasswordTapped(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PushAsync(new ChangePasswordPage());
    }

    private async void OnUpdateProfileClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var phone = PhoneEntry.Text?.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
        {
            await DisplayAlertAsync("Uyarı", "Ad Soyad ve Email alanları boş bırakılamaz.", "Tamam");
            return;
        }

        var (success, message) = await _apiService.UpdateProfileAsync(name, email, phone);
        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");
    }
}
