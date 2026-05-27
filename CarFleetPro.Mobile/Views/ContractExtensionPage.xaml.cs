using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views
{
    public partial class ContractExtensionPage : ContentPage
    {
        private readonly Vehicle _vehicle;

        public ContractExtensionPage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            BindingContext = _vehicle;

            // PropertyChanged olayını dinleyerek girilen gün sayısı değiştikçe yeni bitiş tarihini hesapla
            _vehicle.PropertyChanged += OnVehiclePropertyChanged;
            
            // İlk açılışta tarihi hesapla
            HesaplaYeniBitisTarihi();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vehicle.PropertyChanged -= OnVehiclePropertyChanged;
        }

        private void OnVehiclePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Vehicle.UzatSecilenGunMetni) || e.PropertyName == nameof(Vehicle.KiralamaSuresi))
            {
                HesaplaYeniBitisTarihi();
            }
        }

        private void HesaplaYeniBitisTarihi()
        {
            if (string.IsNullOrEmpty(_vehicle.KiralamaSuresi))
            {
                YeniBitisTarihiLabel.Text = "-";
                return;
            }

            if (DateTime.TryParseExact(_vehicle.KiralamaSuresi, "dd.MM.yyyy", null, DateTimeStyles.None, out var bitisDate))
            {
                int gun = _vehicle.UzatSecilenGunSayisi;
                var yeniBitis = bitisDate.AddDays(gun);
                YeniBitisTarihiLabel.Text = yeniBitis.ToString("dd.MM.yyyy dddd", new CultureInfo("tr-TR"));
            }
            else
            {
                YeniBitisTarihiLabel.Text = "-";
            }
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnUzatOnaylaClicked(object? sender, EventArgs e)
        {
            int gunSayisi = _vehicle.UzatSecilenGunSayisi;
            int maxUzatilabilir = _vehicle.MaksimumUzatilabilirGunSayisi;

            if (gunSayisi <= 0)
            {
                await DisplayAlertAsync("Geçersiz Gün", "Lütfen en az 1 gün giriniz.", "Tamam");
                return;
            }

            if (maxUzatilabilir <= 0)
            {
                await DisplayAlertAsync("Uzatma Yapılamaz", "Bu araç için hemen ertesi güne rezervasyon yapıldığından dolayı sözleşmeyi uzatamazsınız!", "Tamam");
                return;
            }

            if (gunSayisi > maxUzatilabilir)
            {
                await DisplayAlertAsync("Uzatma Sınırı", $"Bu araç için en fazla {maxUzatilabilir} gün uzatma yapabilirsiniz! (İleri tarihli rezervasyon çakışması nedeniyle kısıtlanmıştır)", "Tamam");
                return;
            }

            // Bir önceki sayfadaki ViewModel'i bulup uzatma işlemini çağıralım
            var navigationStack = Navigation.NavigationStack;
            if (navigationStack.Count >= 2)
            {
                var prevPage = navigationStack[navigationStack.Count - 2];
                if (prevPage.BindingContext is GarageViewModel vm)
                {
                    var (success, message) = await vm.UzatSozlesme(_vehicle, gunSayisi);
                    if (success)
                    {
                        await DisplayAlertAsync("Başarılı", $"Sözleşme süresi {gunSayisi} gün uzatıldı. Yeni Bitiş Tarihi: {_vehicle.KiralamaSuresi}", "Tamam");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlertAsync("Hata", message, "Tamam");
                    }
                }
                else
                {
                    await DisplayAlertAsync("Hata", "ViewModel bağlantısı kurulamadı.", "Tamam");
                }
            }
            else
            {
                await Navigation.PopAsync();
            }
        }
    }
}
