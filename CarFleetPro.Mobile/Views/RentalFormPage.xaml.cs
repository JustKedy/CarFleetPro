using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views;

public partial class RentalFormPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Vehicle? _vehicle;
    private List<CustomerName> _customers = new();

    
    public RentalFormPage(Vehicle vehicle)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _vehicle = vehicle;

        VehicleNameLabel.Text  = $"{vehicle.Marka} {vehicle.Model}";
        VehiclePlateLabel.Text = $"{vehicle.Plaka} | {vehicle.Yas} Model";
        GunlukUcretEntry.Text = vehicle.DailyRate.ToString("N2");
        
        // Form açıldığında varsayılan bir hesaplama yapalım
        OnCalculateTotal(this, EventArgs.Empty);
    }

    
    public RentalFormPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa açılırken yumuşak bir geçiş efekti
        this.Opacity = 0;
        await this.FadeToAsync(1, 400, Easing.CubicOut);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCompleteRentalClicked(object? sender, EventArgs e)
    {
        // ####### YUNUS İÇİN NOT #######
        // Kanka buraya kiralama kaydını veritabanına atacak POST isteği gelecek.
        // Formdaki verileri toplayıp ApiService.CreateRentalAsync(request) demelisin.
        // Kayıt başarılıysa kullanıcıyı Dashboard'a veya Garaj'a geri yönlendireceğiz.
        // ##############################

        await DisplayAlertAsync("Bilgi", "Kiralama işlemi başarılı bir şekilde kaydedildi.", "Tamam");
        await Navigation.PopAsync();
    }

    private void OnCalculateTotal(object? sender, EventArgs e)
    {
        if (_vehicle == null) return;
        
        DateTime start = StartDatePicker.Date ?? DateTime.Today;
        DateTime end = EndDatePicker.Date ?? DateTime.Today.AddDays(1);
        
        if (end < start)
        {
            ToplamTutarLabel.Text = "Hatalı Tarih";
            return;
        }

        int days = (end - start).Days;
        if (days <= 0) days = 1; // En az 1 günlük hesaplama

        decimal rentTotal = days * _vehicle.DailyRate;
        decimal depositTotal = decimal.TryParse(DepozitoEntry.Text, out decimal deposit) ? deposit : 0;
        
        decimal finalTotal = rentTotal + depositTotal;
        ToplamTutarLabel.Text = $"{finalTotal:N2} TL";
    }
}