using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views
{
    public partial class RentalFormPage : Microsoft.Maui.Controls.ContentPage
    {
        private readonly ApiService _apiService = new();
        private readonly Vehicle? _vehicle;
        private decimal _bazFiyat = 0;   // Tavan (girilecek max fiyat)
        private decimal _tabanFiyat = 0; // Taban (girilecek min fiyat)

        public RentalFormPage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            BindingContext = _vehicle;

            if (!string.IsNullOrEmpty(vehicle.ResimUrl))
                ImagesCarousel.ItemsSource = new List<string> { vehicle.ResimUrl };

            // Fiyatlar OnAppearing içinde yüklenecek
            StartDatePicker.DateSelected += (s, e) => HesaplaToplamTutar();
            EndDatePicker.DateSelected += (s, e) => HesaplaToplamTutar();
        }

        public RentalFormPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.Opacity = 0;
            await this.FadeToAsync(1, 400, Easing.CubicOut);
            await BelirleAracDurumu();
            await ApplyPricing();
        }

        private async Task ApplyPricing()
        {
            if (_vehicle == null) return;

            decimal baz = 0;
            double maxIndirim = 0;

            // Tüm politikaları tek seferde çek
            var policies = await _apiService.GetPricePoliciesAsync();

            // 1. ADIM: ARACA ÖZEL — PricePolicies tablosunda plakaya göre ara
            var vehiclePolicy = policies.FirstOrDefault(p =>
                p.TargetType == "Vehicle" && p.TargetValue == _vehicle.Plaka);

            if (vehiclePolicy != null)
            {
                baz = vehiclePolicy.BasePrice;
                maxIndirim = vehiclePolicy.MaxDiscountPercentage;
            }
            else
            {
                // 2. ADIM: SEGMENT BAZLI
                var segmentPolicy = policies.FirstOrDefault(p =>
                    p.TargetType == "Segment" && p.TargetValue == _vehicle.Segment);

                if (segmentPolicy != null)
                {
                    baz = segmentPolicy.BasePrice;
                    maxIndirim = segmentPolicy.MaxDiscountPercentage;
                }
                else
                {
                    // 3. ADIM: GLOBAL
                    var globalPolicy = policies.FirstOrDefault(p => p.TargetType == "Global");
                    if (globalPolicy != null)
                    {
                        baz = globalPolicy.BasePrice;
                        maxIndirim = globalPolicy.MaxDiscountPercentage;
                    }
                }
            }

            // Hiç politika yoksa aracın kendi günlük ücretini kullan
            if (baz <= 0) baz = _vehicle.GunlukUcret;
            if (maxIndirim <= 0) maxIndirim = 20; // Varsayılan %20

            var taban = baz * (decimal)(1 - (maxIndirim / 100.0));

            _bazFiyat = baz;
            _tabanFiyat = taban;

            GunlukUcretEntry.Text = baz.ToString("0.##", CultureInfo.InvariantCulture);
            BazFiyatLabel.Text = $"Baz: {baz:N0} ₺";
            TabanFiyatLabel.Text = $"Taban: {taban:N0} ₺";
            
            HesaplaToplamTutar();
        }

        // ─────────────────────────────────────────
        //  ARAÇ DURUMUNA GÖRE PANEL GÖSTERİMİ
        // ─────────────────────────────────────────
        private async Task BelirleAracDurumu()
        {
            if (_vehicle == null) return;

            // Durum: 0=Müsait, 1=Kirada, 2=Bakımda
            int durum = _vehicle.StatusCode; // int değeri

            if (durum == 2) // Bakımda
            {
                MaintenanceWarning.IsVisible = true;
                RentedInfoPanel.IsVisible    = false;
                RentalFormPanel.IsVisible    = false;
            }
            else if (durum == 1) // Kirada
            {
                MaintenanceWarning.IsVisible = false;
                RentedInfoPanel.IsVisible    = true;
                RentalFormPanel.IsVisible    = false;
                await YukleKiraciData();
            }
            else // Müsait
            {
                MaintenanceWarning.IsVisible = false;
                RentedInfoPanel.IsVisible    = false;
                RentalFormPanel.IsVisible    = true;
            }
        }

        // ─────────────────────────────────────────
        //  AKTİF KİRALAMAYI YÜKLE
        // ─────────────────────────────────────────
        private async Task YukleKiraciData()
        {
            if (_vehicle == null) return;
            try
            {
                var rentals = await _apiService.GetRentalsAsync();
                var aktif = rentals.Find(r =>
                    r.VehiclePlate == _vehicle.Plaka &&
                    r.Status == "Aktif");

                if (aktif != null)
                {
                    RenterNameLabel.Text  = aktif.CustomerName;
                    RentStartLabel.Text   = aktif.StartDate.ToString("dd.MM.yyyy");
                    RentEndLabel.Text     = aktif.PlannedEndDate.ToString("dd.MM.yyyy");
                    DailyRateLabel.Text   = $"{aktif.DailyRate:N0} ₺/gün";
                    TotalAmountLabel.Text = $"{aktif.TotalAmount:N0} ₺";
                    RentNotesLabel.Text   = string.IsNullOrWhiteSpace(aktif.Notes) ? "Not yok" : aktif.Notes;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RentalFormPage] Kiracı verisi yüklenemedi: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────
        //  TUTAR HESAPLAMA
        // ─────────────────────────────────────────
        private void OnRateChanged(object? sender, TextChangedEventArgs e)
        {
            if (_bazFiyat <= 0 || _tabanFiyat <= 0)
            {
                HesaplaToplamTutar();
                return;
            }

            if (!decimal.TryParse(GunlukUcretEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal girilen))
            {
                HesaplaToplamTutar();
                return;
            }

            // Tavan (Baz) ve Taban sınırı
            if (girilen > _bazFiyat)
            {
                GunlukUcretEntry.Text = _bazFiyat.ToString("0.##", CultureInfo.InvariantCulture);
                GunlukUcretEntry.CursorPosition = GunlukUcretEntry.Text.Length;
                return; // TextChanged yeniden tetiklenecek
            }

            if (girilen < _tabanFiyat)
            {
                GunlukUcretEntry.Text = _tabanFiyat.ToString("0.##", CultureInfo.InvariantCulture);
                GunlukUcretEntry.CursorPosition = GunlukUcretEntry.Text.Length;
                return;
            }

            HesaplaToplamTutar();
        }

        private void HesaplaToplamTutar()
        {
            try
            {
                var start = StartDatePicker.Date.GetValueOrDefault(DateTime.Today);
                var end   = EndDatePicker.Date.GetValueOrDefault(DateTime.Today.AddDays(1));
                int gun   = Math.Max((end - start).Days, 1);

                if (decimal.TryParse(GunlukUcretEntry.Text,
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal gunluk))
                {
                    decimal depozito = 0;
                    if (!string.IsNullOrWhiteSpace(DepozitoEntry.Text))
                        decimal.TryParse(DepozitoEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out depozito);

                    ToplamTutarLabel.Text = $"{(gunluk * gun) + depozito:N2} ₺";
                }
                else
                {
                    decimal depozito = 0;
                    if (!string.IsNullOrWhiteSpace(DepozitoEntry.Text))
                        decimal.TryParse(DepozitoEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out depozito);
                        
                    ToplamTutarLabel.Text = $"{depozito:N2} ₺";
                }
            }
            catch { ToplamTutarLabel.Text = "0.00 ₺"; }
        }

        // ─────────────────────────────────────────
        //  FORMU KAYDET
        // ─────────────────────────────────────────
        private async void OnCompleteRentalClicked(object? sender, EventArgs e)
        {
            // Zorunlu alan kontrolleri
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text))
            { await DisplayAlertAsync("Uyarı", "Kiracının adını giriniz.", "Tamam"); return; }

            if (string.IsNullOrWhiteSpace(LastNameEntry.Text))
            { await DisplayAlertAsync("Uyarı", "Kiracının soyadını giriniz.", "Tamam"); return; }

            if (string.IsNullOrWhiteSpace(TcEntry.Text) || TcEntry.Text.Length != 11)
            { await DisplayAlertAsync("Uyarı", "Geçerli bir 11 haneli T.C. Kimlik No giriniz.", "Tamam"); return; }

            if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
            { await DisplayAlertAsync("Uyarı", "Telefon numarasını giriniz.", "Tamam"); return; }

            if (string.IsNullOrWhiteSpace(LicenseNoEntry.Text))
            { await DisplayAlertAsync("Uyarı", "Ehliyet seri numarasını giriniz.", "Tamam"); return; }

            if (_vehicle == null)
            { await DisplayAlertAsync("Hata", "Araç bilgisi bulunamadı.", "Tamam"); return; }

            // Fiyat validasyonu
            if (!decimal.TryParse(GunlukUcretEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal girilenFiyat))
            { await DisplayAlertAsync("Uyarı", "Geçerli bir günlük ücret giriniz.", "Tamam"); return; }

            decimal bazFiyat = _vehicle.GunlukUcret;
            decimal tabanFiyat = bazFiyat * 0.8m;

            if (girilenFiyat > bazFiyat)
            { await DisplayAlertAsync("Uyarı", $"Günlük ücret baz fiyattan ({bazFiyat:N2} ₺) yüksek olamaz.", "Tamam"); return; }

            if (girilenFiyat < tabanFiyat)
            { await DisplayAlertAsync("Uyarı", $"Günlük ücret taban fiyattan ({tabanFiyat:N2} ₺) düşük olamaz.", "Tamam"); return; }

            var startDate = StartDatePicker.Date.GetValueOrDefault(DateTime.Today);
            var endDate   = EndDatePicker.Date.GetValueOrDefault(DateTime.Today.AddDays(1));

            if (endDate <= startDate)
            { await DisplayAlertAsync("Uyarı", "Dönüş tarihi teslim tarihinden sonra olmalıdır.", "Tamam"); return; }

            decimal depositAmount = 0;
            if (!string.IsNullOrWhiteSpace(DepozitoEntry.Text))
                decimal.TryParse(DepozitoEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out depositAmount);

            var notes = NotesEditor.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(EmergencyNameEntry.Text))
                notes += $"\nAcil İletişim: {EmergencyNameEntry.Text} — {EmergencyPhoneEntry.Text}";

            KiralaBtn.IsEnabled = false;
            KiralaBtn.Text      = "Kaydediliyor...";

            var (success, message) = await _apiService.CreateRentalWithGuestAsync(
                firstName:     FirstNameEntry.Text,
                lastName:      LastNameEntry.Text,
                phone:         PhoneEntry.Text,
                vehicleId:     _vehicle.Id,
                startDate:     startDate,
                endDate:       endDate,
                depositAmount: depositAmount,
                notes:         notes,
                tc:            TcEntry.Text,
                licenseNo:     LicenseNoEntry.Text,
                licenseExpiry: LicenseExpiryPicker.Date.GetValueOrDefault(DateTime.Today.AddYears(5)),
                address:       AddressEditor.Text ?? "Belirtilmedi");

            KiralaBtn.IsEnabled = true;
            KiralaBtn.Text      = "KİRALAMAYI TAMAMLA";

            if (success)
            {
                await DisplayAlertAsync("Başarılı", "Kiralama işlemi başarıyla kaydedildi.", "Tamam");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlertAsync("Hata", message, "Tamam");
            }
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnPrevImageClicked(object? sender, EventArgs e)
        {
            if (ImagesCarousel.ItemsSource is IList<string> items && items.Count > 0)
            {
                int i = ImagesCarousel.Position;
                ImagesCarousel.Position = i > 0 ? i - 1 : items.Count - 1;
            }
        }

        private void OnNextImageClicked(object? sender, EventArgs e)
        {
            if (ImagesCarousel.ItemsSource is IList<string> items && items.Count > 0)
            {
                int i = ImagesCarousel.Position;
                ImagesCarousel.Position = i < items.Count - 1 ? i + 1 : 0;
            }
        }
    }
}
