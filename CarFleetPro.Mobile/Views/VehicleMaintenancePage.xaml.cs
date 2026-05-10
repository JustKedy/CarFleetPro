using CarFleetPro.Mobile.Models;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class VehicleMaintenancePage : ContentPage
    {
        private readonly Vehicle? _vehicle;

        public VehicleMaintenancePage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            DoldurAracBilgileri(vehicle);
        }

        public VehicleMaintenancePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.Opacity = 0;
            await this.FadeToAsync(1, 400, Easing.CubicOut);
        }

        private void DoldurAracBilgileri(Vehicle v)
        {
            AracAdLabel.Text    = $"{v.Marka} {v.Model}";
            AracPlakaLabel.Text = v.Plaka;
            MarkaModelLabel.Text = $"{v.Marka} {v.Model}";
            KmLabel.Text        = $"{v.Km:N0} KM";
            // Yaş → yıl hesabı: Yas alanı "araç kaç yıllık" demek
            int yil = DateTime.Now.Year - v.Yas;
            YilLabel.Text   = yil > 1900 ? yil.ToString() : $"{v.Yas} Yıl";
            DurumLabel.Text = v.Durum;

            if (!string.IsNullOrEmpty(v.ResimUrl))
                AracResim.Source = v.ResimUrl;
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnBakimKaydetClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BakimTuruEntry.Text))
            {
                await DisplayAlert("Uyarı", "Lütfen bakım türünü giriniz.", "Tamam");
                return;
            }

            if (_vehicle != null)
            {
                var apiService = new CarFleetPro.Mobile.Services.ApiService();
                var result = await apiService.SendToMaintenanceAsync(_vehicle.Id);
                
                if (result.Success)
                {
                    _vehicle.Durum = "BAKIMDA";
                    await DisplayAlert("Başarılı", $"{_vehicle.Plaka} bakıma alındı.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", result.Message ?? "Araç bakıma alınırken bir sorun oluştu.", "Tamam");
                }
            }
        }
    }
}
