using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views
{
    public partial class AddReservationPage : ContentPage
    {
        private readonly Vehicle _vehicle;

        public AddReservationPage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            BindingContext = _vehicle;

            SetupTarihKisitlari();
        }

        private void SetupTarihKisitlari()
        {
            // Dinamik minimum başlangıç tarihi kısıtı ayarlama
            if (DateTime.TryParseExact(_vehicle.RezervasyonBaslangicMinTarihi, "yyyy-MM-dd", null, DateTimeStyles.None, out var minDate))
            {
                RezBaslangicDatePicker.MinimumDate = minDate;
                _vehicle.RezBaslangicTarihi = minDate;
                MinTarihLabel.Text = minDate.ToString("dd.MM.yyyy");
            }
            else
            {
                var varsayilanMinDate = DateTime.Today.AddDays(6);
                RezBaslangicDatePicker.MinimumDate = varsayilanMinDate;
                _vehicle.RezBaslangicTarihi = varsayilanMinDate;
                MinTarihLabel.Text = varsayilanMinDate.ToString("dd.MM.yyyy");
            }
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnRezervasyonOnaylaClicked(object? sender, EventArgs e)
        {
            var musteriAdi = _vehicle.RezMusteriAdi;
            var musteriTelefon = _vehicle.RezMusteriTelefon;
            var baslangicTarihi = _vehicle.RezBaslangicTarihi;
            
            if (string.IsNullOrWhiteSpace(musteriAdi))
            {
                await DisplayAlertAsync("Hata", "Lütfen müşteri adını giriniz.", "Tamam");
                return;
            }

            if (string.IsNullOrWhiteSpace(musteriTelefon))
            {
                await DisplayAlertAsync("Hata", "Lütfen müşteri telefon numarasını giriniz.", "Tamam");
                return;
            }

            if (!int.TryParse(_vehicle.RezGunSuresi, out int gunSuresi) || gunSuresi <= 0)
            {
                await DisplayAlertAsync("Hata", "Lütfen geçerli bir gün süresi giriniz.", "Tamam");
                return;
            }

            // Minimum tarih kontrolü
            if (DateTime.TryParseExact(_vehicle.RezervasyonBaslangicMinTarihi, "yyyy-MM-dd", null, DateTimeStyles.None, out var minDate))
            {
                if (baslangicTarihi.Date < minDate.Date)
                {
                    await DisplayAlertAsync("Geçersiz Tarih", $"İleri tarihli kiralama en erken {minDate:dd.MM.yyyy} tarihinde başlayabilir.", "Tamam");
                    return;
                }
            }

            // Bir önceki sayfadaki ViewModel'i bulup rezervasyon ekleme işlemini çağıralım
            var navigationStack = Navigation.NavigationStack;
            if (navigationStack.Count >= 2)
            {
                var prevPage = navigationStack[navigationStack.Count - 2];
                if (prevPage.BindingContext is GarageViewModel vm)
                {
                    // Butonları devre dışı bırakıp bekleme durumu gösterebiliriz veya doğrudan çağırabiliriz
                    var (success, message) = await vm.EkleRezervasyon(_vehicle, musteriAdi, musteriTelefon, baslangicTarihi, gunSuresi);
                    
                    if (success)
                    {
                        // Formu temizle
                        _vehicle.RezMusteriAdi = string.Empty;
                        _vehicle.RezMusteriTelefon = string.Empty;
                        _vehicle.RezGunSuresi = "1";

                        await DisplayAlertAsync("Rezervasyon Başarılı", $"{musteriAdi} adına ileri tarihli rezervasyon kaydı başarıyla oluşturuldu.", "Tamam");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlertAsync("Rezervasyon Başarısız", $"Rezervasyon kaydı oluşturulamadı: {message}", "Tamam");
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
