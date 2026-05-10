using CarFleetPro.Mobile.Models;
using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class DamageRecordPage : ContentPage
    {
        private readonly Vehicle? _vehicle;

        public DamageRecordPage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            DoldurAracBilgileri(vehicle);
        }

        public DamageRecordPage()
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

        private async void OnHasarKaydetClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HasarTuruEntry.Text))
            {
                await DisplayAlert("Uyarı", "Lütfen hasar türünü giriniz.", "Tamam");
                return;
            }

            // Burada şu anlık sadece Alert gösteriliyor. İleride API'ye bağlanabilir.
            if (_vehicle != null)
            {
                await DisplayAlert("Başarılı", $"{_vehicle.Plaka} aracı için hasar kaydı eklendi.", "Tamam");
                await Navigation.PopAsync();
            }
        }
    }
}
