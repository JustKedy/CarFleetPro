using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.Views
{
    public partial class VehicleMaintenancePage : ContentPage
    {
        private readonly Vehicle? _vehicle;
        private readonly ApiService _apiService;

        // Araç parametresiyle açılış (FleetManagement'dan)
        public VehicleMaintenancePage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle   = vehicle;
            _apiService = new ApiService();
            DoldurAracBilgileri(vehicle);
        }

        // Parametresiz açılış (GaragePage vb.)
        public VehicleMaintenancePage()
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

            await YukleBakimKayitlari();
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
        //  API'DEN BAKIM KAYITLARINI YÜKle + FİLTRELE
        // ─────────────────────────────────────────
        private async Task YukleBakimKayitlari()
        {
            YukleniyorGostergesi.IsRunning = true;
            YukleniyorGostergesi.IsVisible = true;
            ListGrid.IsVisible             = false;

            try
            {
                var tumKayitlar = await _apiService.GetMaintenancesAsync();

                // Eğer belirli bir araç için açıldıysa sadece onun kayıtlarını göster
                var kayitlar = (_vehicle != null)
                    ? tumKayitlar.Where(m => m.VehicleId == _vehicle.Id)
                                 .OrderByDescending(m => m.StartDate)
                                 .ToList()
                    : tumKayitlar.OrderByDescending(m => m.StartDate).ToList();

                MaintenanceList.ItemsSource = kayitlar;

                KayitSayisiLabel.Text = kayitlar.Count == 0
                    ? "Henüz bakım kaydı yok"
                    : $"{kayitlar.Count} kayıt listeleniyor";

                BosListePanel.IsVisible    = kayitlar.Count == 0;
                MaintenanceList.IsVisible  = kayitlar.Count > 0;
            }
            catch (Exception ex)
            {
                KayitSayisiLabel.Text = "Kayıtlar yüklenemedi";
                System.Diagnostics.Debug.WriteLine($"[VehicleMaintenancePage] Yükleme hatası: {ex.Message}");
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
            BakimTuruEntry.Text      = string.Empty;
            MaliyetEntry.Text        = string.Empty;
            SonrakiBakimKmEntry.Text = string.Empty;
            AciklamaEditor.Text      = string.Empty;
            BakimTarihiPicker.Date   = DateTime.Today;

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
        //  FORM: BAKIM KAYDET
        // ─────────────────────────────────────────
        private async void OnBakimKaydetClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BakimTuruEntry.Text))
            {
                await DisplayAlertAsync("Uyarı", "Lütfen bakım / arıza türünü giriniz.", "Tamam");
                return;
            }

            if (_vehicle == null)
            {
                await DisplayAlertAsync("Hata", "Araç bilgisi bulunamadı.", "Tamam");
                return;
            }

            decimal cost = 0;
            if (!string.IsNullOrWhiteSpace(MaliyetEntry.Text) &&
                !decimal.TryParse(MaliyetEntry.Text, System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture, out cost))
            {
                await DisplayAlertAsync("Uyarı", "Geçerli bir maliyet tutarı giriniz (Ör: 1500 veya 1500.00).", "Tamam");
                return;
            }

            var description = AciklamaEditor.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(SonrakiBakimKmEntry.Text))
                description += $"\nSonraki Bakım KM: {SonrakiBakimKmEntry.Text}";

            var request = new CreateMaintenanceRequest
            {
                VehicleId       = _vehicle.Id,
                MaintenanceType = BakimTuruEntry.Text.Trim(),
                Description     = description.Trim(),
                StartDate       = BakimTarihiPicker.Date.GetValueOrDefault(DateTime.Today),
                Cost            = cost,
                NextInspectionDate = null
            };

            BakimKaydetBtn.IsEnabled = false;
            BakimKaydetBtn.Text      = "Kaydediliyor...";

            var (success, message) = await _apiService.CreateMaintenanceAsync(request);

            BakimKaydetBtn.IsEnabled = true;
            BakimKaydetBtn.Text      = "BAKIMI KAYDET";

            if (success)
            {
                await DisplayAlertAsync("Başarılı", $"{_vehicle.Plaka} plakalı araç için bakım kaydı eklendi.", "Tamam");

                // Formu kapat, listeyi yenile
                FormScrollView.IsVisible = false;
                ListGrid.IsVisible       = true;
                await YukleBakimKayitlari();
            }
            else
            {
                await DisplayAlertAsync("Hata", message, "Tamam");
            }
        }
    }
}
