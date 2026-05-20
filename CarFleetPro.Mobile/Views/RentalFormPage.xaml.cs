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
            await LoadVehicleImages();
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

            // Hiç politika yoksa aracın kendi günlük ücretini veya araçta tanımlı baz fiyatı kullan
            if (baz <= 0)
            {
                baz = _vehicle.BasePrice > 0 ? _vehicle.BasePrice : _vehicle.GunlukUcret;
            }
            
            // Politikalardan veya araçtan gelen bir indirim oranı yoksa varsayılan olarak %5 kullan
            if (maxIndirim <= 0)
            {
                maxIndirim = _vehicle.MaxDiscountPercentage > 0 ? _vehicle.MaxDiscountPercentage : 5;
            }

            // Taban fiyat, baz fiyattan maksimum indirim oranı düşülerek dinamik olarak hesaplanır.
            var taban = baz - (baz * (decimal)(maxIndirim / 100.0));

            _bazFiyat = baz;
            _tabanFiyat = taban;

            GunlukUcretEntry.Text = baz.ToString("0.##", CultureInfo.InvariantCulture);
            BazFiyatLabel.Text = $"Baz: {baz:N0} ₺";
            TabanFiyatLabel.Text = $"Taban: {taban:N0} ₺";
            MaxIndirimLabel.Text = $"Maks. İndirim: %{maxIndirim:0.##}";
            
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
                    
                    // Kiralama tarih ve fiyat bilgilerini doldur
                    RentStartLabel.Text   = aktif.StartDate.ToString("dd.MM.yyyy");
                    RentEndLabel.Text     = aktif.PlannedEndDate.ToString("dd.MM.yyyy");
                    DailyRateLabel.Text   = $"{aktif.DailyRate:N0} ₺/gün";
                    TotalAmountLabel.Text = $"{aktif.TotalAmount:N0} ₺";
                    RentNotesLabel.Text   = string.IsNullOrWhiteSpace(aktif.Notes) ? "Not yok" : aktif.Notes;

                    // Müşteri bilgileri (kiralama nesnesinden gelenler)
                    string tc = string.IsNullOrWhiteSpace(aktif.CustomerIdentityNumber) ? "-" : aktif.CustomerIdentityNumber;
                    string phone = string.IsNullOrWhiteSpace(aktif.CustomerPhone) ? "-" : aktif.CustomerPhone;
                    string license = string.IsNullOrWhiteSpace(aktif.CustomerDriverLicenseNumber) ? "-" : aktif.CustomerDriverLicenseNumber;
                    string licenseExpiry = aktif.CustomerDriverLicenseExpiry == default ? "-" : aktif.CustomerDriverLicenseExpiry.ToString("dd.MM.yyyy");
                    string address = string.IsNullOrWhiteSpace(aktif.CustomerAddress) ? "Belirtilmedi" : aktif.CustomerAddress;

                    // Canlıda yayındaki API'nin güncellenmemiş sürüm olması durumuna karşı
                    // Eğer bilgiler boş ise müşteriyi ismiyle arayıp detayını çekiyoruz.
                    if (tc == "-" || phone == "-" || license == "-" || address == "Belirtilmedi")
                    {
                        try
                        {
                            var searchResults = await _apiService.SearchCustomersAsync(aktif.CustomerName);
                            if (searchResults != null && searchResults.Count > 0)
                            {
                                // İsmi tam eşleşen müşteriyi bulalım
                                var matched = searchResults.Find(c => 
                                    c.FullName.Trim().Equals(aktif.CustomerName.Trim(), StringComparison.OrdinalIgnoreCase));
                                
                                int targetCustId = matched != null ? matched.CustomerId : searchResults[0].CustomerId;

                                var detail = await _apiService.GetCustomerDetailAsync(targetCustId);
                                if (detail != null)
                                {
                                    tc = string.IsNullOrWhiteSpace(detail.IdentityNumber) ? "-" : detail.IdentityNumber;
                                    phone = string.IsNullOrWhiteSpace(detail.PhoneNumber) ? "-" : detail.PhoneNumber;
                                    license = string.IsNullOrWhiteSpace(detail.DriverLicenseNumber) ? "-" : detail.DriverLicenseNumber;
                                    licenseExpiry = detail.DriverLicenseExpiry == default ? "-" : detail.DriverLicenseExpiry.ToString("dd.MM.yyyy");
                                    address = string.IsNullOrWhiteSpace(detail.Address) ? "Belirtilmedi" : detail.Address;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[RentalFormPage] Müşteri detay tamamlama hatası: {ex.Message}");
                        }
                    }

                    // Alanları ekrana bas
                    RenterTcLabel.Text    = tc;
                    RenterPhoneLabel.Text = phone;
                    RenterLicenseLabel.Text = license;
                    RenterLicenseExpiryLabel.Text = licenseExpiry;
                    RenterAddressLabel.Text = address;
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
            if (decimal.TryParse(GunlukUcretEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal girilenFiyat))
            {
                if (_bazFiyat > 0 && girilenFiyat > _bazFiyat)
                {
                    // Baz fiyattan yüksek girilmesini anında engelle
                    GunlukUcretEntry.Text = _bazFiyat.ToString("0.##", CultureInfo.InvariantCulture);
                }
            }
            HesaplaToplamTutar();
        }

        private void OnRateUnfocused(object? sender, FocusEventArgs e)
        {
            if (decimal.TryParse(GunlukUcretEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal girilenFiyat))
            {
                if (_tabanFiyat > 0 && girilenFiyat < _tabanFiyat)
                {
                    // Taban fiyattan düşük girilmesini odaktan çıkınca engelle
                    GunlukUcretEntry.Text = _tabanFiyat.ToString("0.##", CultureInfo.InvariantCulture);
                }
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

            if (_bazFiyat > 0 && girilenFiyat > _bazFiyat)
            { 
                GunlukUcretEntry.Text = _bazFiyat.ToString("0.##", CultureInfo.InvariantCulture);
                await DisplayAlertAsync("Uyarı", $"Günlük ücret baz fiyattan ({_bazFiyat:N0} ₺) yüksek olamaz. Fiyat otomatik olarak limit değerine çekildi.", "Tamam"); 
                return; 
            }

            if (_tabanFiyat > 0 && girilenFiyat < _tabanFiyat)
            { 
                GunlukUcretEntry.Text = _tabanFiyat.ToString("0.##", CultureInfo.InvariantCulture);
                await DisplayAlertAsync("Uyarı", $"Günlük ücret taban fiyattan ({_tabanFiyat:N0} ₺) düşük olamaz. Fiyat otomatik olarak limit değerine çekildi.", "Tamam"); 
                return; 
            }

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

        private async Task LoadVehicleImages()
        {
            if (_vehicle == null) return;
            try
            {
                var images = await _apiService.GetVehicleImagesAsync(_vehicle.Id);
                var imageUrls = new List<string>();

                if (images != null && images.Count > 0)
                {
                    var sortedImages = images.OrderByDescending(img => img.IsPrimary)
                                             .ThenBy(img => img.DisplayOrder)
                                             .Select(img => img.ImageUrl)
                                             .ToList();
                    imageUrls.AddRange(sortedImages);
                }

                if (imageUrls.Count == 0 && !string.IsNullOrEmpty(_vehicle.ResimUrl))
                {
                    imageUrls.Add(_vehicle.ResimUrl);
                }

                if (imageUrls.Count == 0)
                {
                    imageUrls.Add("car.svg");
                }

                ImagesCarousel.ItemsSource = imageUrls;
                UpdateImageCounter(0, imageUrls.Count);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RentalFormPage] Fotoğraflar yüklenirken hata: {ex.Message}");
            }
        }

        private void UpdateImageCounter(int position, int total)
        {
            if (total <= 0)
            {
                ImageCounterLabel.Text = "0 / 0";
                return;
            }
            ImageCounterLabel.Text = $"{position + 1} / {total}";
        }

        private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
        {
            if (ImagesCarousel.ItemsSource is System.Collections.ICollection collection)
            {
                UpdateImageCounter(e.CurrentPosition, collection.Count);
            }
        }
    }
}
