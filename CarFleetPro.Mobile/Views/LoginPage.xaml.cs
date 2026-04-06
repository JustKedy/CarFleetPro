using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    // YUNUS İÇİN NOT: İleride buraya API şifre kontrolü gelecek, şimdilik direkt Ana Sayfaya atıyoruz.
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
    }
}