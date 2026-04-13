using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage()
    {
        InitializeComponent();
    }

    // Giriş ekranına dön yazısına tıklandığında çalışacak motor
    private async void OnBackToLoginTapped(object? sender, EventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }
}