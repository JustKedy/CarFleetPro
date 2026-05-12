using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.Views
{
    public partial class DamageRecordPage : ContentPage
    {
        private readonly Vehicle? _vehicle;
        private readonly ApiService _apiService;

        // Araç parametresiyle açılış
        public DamageRecordPage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle    = vehicle;
            _apiService = new ApiService();
            DoldurAracBilgileri(vehicle);
        }

        // Parametresiz açılış
        public DamageRecordPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        // ─────────────────────────────────────────
        //  SAYFA AÇILDIĞINDA: kayıtları yükle
        // ─────────────────────────────────────────
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.Opacity = 0;
            await this.FadeToAsync(1, 350, Easing.CubicOut);

            await YukleHasarKayitlari();
        }

        // ─────────────────────────────────────────
        //  ARAÇ BİLGİLERİNİ HEADER'A DOLDUR
        // ─────────────────────────────────────────
        private void DoldurAracBilgileri(Vehicle v)
        {
            AracAdLabel.Text     = $"{v.Marka} {v.Model}";
            AracPlakaLabel.Text  = v.Plaka;
            FormAracPlaka.Text   = $"{v.Marka} {v.Model} — {v.Plaka}";
            KmLabel.Text         = $"{v.Km:N0}";
            int yil = DateTime.Now.Year - v.Yas;
            YilLabel.Text        = yil > 1900 ? yil.ToString() : $"{v.Yas}";
            DurumLabel.Text      = v.Durum;

            if (!string.IsNullOrEmpty(v.ResimUrl))
                AracResimMini.Source = v.ResimUrl;
        }

        // ─────────────────────────────────────────
        //  API'DEN HASAR KAYITLARINI YÜKLE + FİLTRELE
        // ─────────────────────────────────────────
        private async Task YukleHasarKayitlari()
        {
            YukleniyorGostergesi.IsRunning = true;
            YukleniyorGostergesi.IsVisible = true;
            ListGrid.IsVisible             = false;

            try
            {
                var tumKayitlar = await _apiService.GetDamageRecordsAsync();

                // Eğer belirli bir araç için açıldıysa sadece onun kayıtlarını göster
                var kayitlar = (_vehicle != null)
                    ? tumKayitlar.Where(d => d.VehicleId == _vehicle.Id)
                                 .OrderByDescending(d => d.DamageDate)
                                 .ToList()
                    : tumKayitlar.OrderByDescending(d => d.DamageDate).ToList();

                DamageList.ItemsSource = kayitlar;

                KayitSayisiLabel.Text = kayitlar.Count == 0
                    ? "Henüz hasar kaydı yok"
                    : $"{kayitlar.Count} kayıt listeleniyor";

                BosListePanel.IsVisible = kayitlar.Count == 0;
                DamageList.IsVisible    = kayitlar.Count > 0;
            }
            catch (Exception ex)
            {
                KayitSayisiLabel.Text = "Kayıtlar yüklenemedi";
                System.Diagnostics.Debug.WriteLine($"[DamageRecordPage] Yükleme hatası: {ex.Message}");
            }
            finally
            {
                YukleniyorGostergesi.IsRunning = false;
                YukleniyorGostergesi.IsVisible = false;
                ListGrid.IsVisible             = true;
            }
        }

        // ─────────────────────────────────────────
        //  BUTON: Yeni Kayıt Ekle → FORMU AÇ
        // ─────────────────────────────────────────
        private async void OnYeniKayitClicked(object? sender, EventArgs e)
        {
            // Formu temizle
            HasarTuruEntry.Text     = string.Empty;
            MaliyetEntry.Text       = string.Empty;
            AciklamaEditor.Text     = string.Empty;
            HasarTarihiPicker.Date  = DateTime.Today;

            // Animate geçiş
            ListGrid.IsVisible       = false;
            FormScrollView.IsVisible = true;
            FormScrollView.Opacity   = 0;
            await FormScrollView.FadeToAsync(1, 250, Easing.CubicOut);
        }

        // ─────────────────────────────────────────
        //  BUTON: Formdan geri → LİSTEYE DÖN
        // ─────────────────────────────────────────
        private async void OnFormGeriClicked(object? sender, EventArgs e)
        {
            await FormScrollView.FadeToAsync(0, 200, Easing.CubicIn);
            FormScrollView.IsVisible = false;
            ListGrid.IsVisible       = true;
        }

        // ─────────────────────────────────────────
        //  BUTON: Geri (header)
        // ─────────────────────────────────────────
        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // ─────────────────────────────────────────
        //  FORM: HASAR KAYDET
        // ─────────────────────────────────────────
        private async void OnHasarKaydetClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HasarTuruEntry.Text))
            {
                await DisplayAlertAsync("Uyarı", "Lütfen hasar türünü giriniz.", "Tamam");
                return;
            }

            if (_vehicle == null)
            {
                await DisplayAlertAsync("Hata", "Araç bilgisi bulunamadı.", "Tamam");
                return;
            }

            decimal estimatedCost = 0;
            if (!string.IsNullOrWhiteSpace(MaliyetEntry.Text) &&
                !decimal.TryParse(MaliyetEntry.Text, System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture, out estimatedCost))
            {
                await DisplayAlertAsync("Uyarı", "Geçerli bir maliyet tutarı giriniz (Ör: 2500 veya 2500.00).", "Tamam");
                return;
            }

            var request = new CreateDamageRecordRequest
            {
                VehicleId     = _vehicle.Id,
                DamageType    = HasarTuruEntry.Text.Trim(),
                Description   = AciklamaEditor.Text?.Trim() ?? string.Empty,
                DamageDate    = HasarTarihiPicker.Date.GetValueOrDefault(DateTime.Today),
                EstimatedCost = estimatedCost,
                PhotoUrl      = null
            };

            HasarKaydetBtn.IsEnabled = false;
            HasarKaydetBtn.Text      = "Kaydediliyor...";

            var (success, message) = await _apiService.CreateDamageRecordAsync(request);

            HasarKaydetBtn.IsEnabled = true;
            HasarKaydetBtn.Text      = "HASARI KAYDET";

            if (success)
            {
                await DisplayAlertAsync("Başarılı", $"{_vehicle.Plaka} plakalı araç için hasar kaydı eklendi.", "Tamam");

                // Formu kapat, listeyi yenile
                FormScrollView.IsVisible = false;
                ListGrid.IsVisible       = true;
                await YukleHasarKayitlari();
            }
            else
            {
                await DisplayAlertAsync("Hata", message, "Tamam");
            }
        }
    }
}
