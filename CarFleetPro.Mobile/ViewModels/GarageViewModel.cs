using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace CarFleetPro.Mobile.ViewModels
{
    public partial class GarageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Vehicle> AracListesi { get; set; } = [];
        private List<Vehicle> _tumAraclar = [];

        [ObservableProperty] public partial bool IsLoading { get; set; } = true;

        [ObservableProperty] public partial bool IsTumuSelected { get; set; } = true;
        [ObservableProperty] public partial bool IsMusaitSelected { get; set; } = false;
        [ObservableProperty] public partial bool IsDoluSelected { get; set; } = false;
        [ObservableProperty] public partial bool IsBakimdaSelected { get; set; } = false;

        public ObservableCollection<SegmentFilterItem> SegmentFilters { get; set; } = [];
        public ObservableCollection<SegmentFilterItem> BrandFilters { get; set; } = [];

        [ObservableProperty] public partial bool IsFilterPanelExpanded { get; set; } = false;

        public GarageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadVehiclesFromApi();
        }

        private async Task LoadVehiclesFromApi(bool forceRefresh = false)
        {
            IsLoading = true;
            try
            {
                var apiVehicles = await _apiService.GetVehiclesAsync(forceRefresh);
                var apiSegmentler = await _apiService.GetCarTypesAsync();
                var apiMarkalar = await _apiService.GetBrandsAsync();

                if (apiVehicles == null || apiVehicles.Count == 0)
                    throw new Exception("API boş liste döndürdü.");

                List<RentalInfo> rentals = new();
                try
                {
                    rentals = await _apiService.GetRentalsAsync();
                }
                catch (Exception rx)
                {
                    System.Diagnostics.Debug.WriteLine($"Kiralama listesi çekilemedi: {rx.Message}");
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _tumAraclar.Clear();
                    foreach (var v in apiVehicles)
                    {
                        if (v.StatusCode == 1) // Kirada/Dolu
                        {
                            var aktif = rentals.FirstOrDefault(r => 
                                !string.IsNullOrEmpty(r.VehiclePlate) && 
                                r.VehiclePlate.Trim().Equals(v.Plaka.Trim(), StringComparison.OrdinalIgnoreCase) && 
                                !string.IsNullOrEmpty(r.Status) && 
                                r.Status.Trim().Equals("Aktif", StringComparison.OrdinalIgnoreCase));
                                
                            if (aktif != null)
                            {
                                v.KiralayanKisi = aktif.CustomerName;
                                v.KiralamaFiyati = aktif.TotalAmount;
                                v.KiralamaTarihi = aktif.StartDate.ToString("dd.MM.yyyy");
                                v.KiralamaSuresi = aktif.PlannedEndDate.ToString("dd.MM.yyyy");
                                v.KiralayanTelefon = string.IsNullOrWhiteSpace(aktif.CustomerPhone) ? "-" : aktif.CustomerPhone;
                                v.KiralayanTc = string.IsNullOrWhiteSpace(aktif.CustomerIdentityNumber) ? "-" : aktif.CustomerIdentityNumber;
                                v.KiralayanEhliyetNo = string.IsNullOrWhiteSpace(aktif.CustomerDriverLicenseNumber) ? "-" : aktif.CustomerDriverLicenseNumber;
                                v.KiralayanEhliyetGecerlilik = aktif.CustomerDriverLicenseExpiry == default ? "-" : aktif.CustomerDriverLicenseExpiry.ToString("dd.MM.yyyy");
                                v.KiralayanAdres = string.IsNullOrWhiteSpace(aktif.CustomerAddress) ? "Belirtilmedi" : aktif.CustomerAddress;
                                v.KiralayanNotlar = string.IsNullOrWhiteSpace(aktif.Notes) ? "Not yok" : aktif.Notes;
                            }
                        }
                        _tumAraclar.Add(v);
                    }

                    var mevcutSegmentler = SegmentFilters.Select(s => s.Name).ToHashSet();
                    foreach (var seg in apiSegmentler)
                    {
                        if (!mevcutSegmentler.Contains(seg.Name))
                        {
                            SegmentFilters.Add(new SegmentFilterItem { Name = seg.Name, IsSelected = false });
                        }
                    }

                    var mevcutMarkalar = BrandFilters.Select(s => s.Name).ToHashSet();
                    foreach (var marka in apiMarkalar)
                    {
                        if (!mevcutMarkalar.Contains(marka.Name))
                        {
                            BrandFilters.Add(new SegmentFilterItem { Name = marka.Name, IsSelected = false });
                        }
                    }

                    FiltreUygula();
                });
            }
            catch (Exception ex)
            {
                var hataMesaji = ex.InnerException?.Message ?? ex.Message;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AracListesi.Clear();
                    AracListesi.Add(new Vehicle
                    {
                        Id     = -1,
                        Plaka  = "BAĞLANTI HATASI",
                        Marka  = hataMesaji,
                        Model  = ex.GetType().Name,
                        Durum  = "BAKIMDA",
                    });
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task VerileriYenile() => await LoadVehiclesFromApi(forceRefresh: true);

        [RelayCommand]
        public void DetayAcKapa(Vehicle? selectedVehicle)
        {
            if (selectedVehicle is null) return;
            selectedVehicle.IsExpanded = !selectedVehicle.IsExpanded;
        }

        [RelayCommand]
        public void ToggleFilterPanel()
        {
            IsFilterPanelExpanded = !IsFilterPanelExpanded;
        }

        [RelayCommand]
        public void SegmentToggled()
        {
            FiltreUygula();
        }

        [RelayCommand]
        public void Filtrele(string durum)
        {
            IsTumuSelected = durum == "Tümü";
            IsMusaitSelected = durum == "Müsait";
            IsDoluSelected = durum == "Dolu";
            IsBakimdaSelected = durum == "Bakımda";
            FiltreUygula();
        }

        private void FiltreUygula()
        {
            AracListesi.Clear();
            var seciliSegmentler = SegmentFilters.Where(s => s.IsSelected).Select(s => s.Name).ToList();
            var seciliMarkalar = BrandFilters.Where(s => s.IsSelected).Select(s => s.Name).ToList();

            foreach (var vehicle in _tumAraclar)
            {
                bool durumUyuyor = false;
                if (IsTumuSelected)
                {
                    durumUyuyor = true;
                }
                else if (IsMusaitSelected && (vehicle.Durum?.Equals("MÜSAİT", StringComparison.OrdinalIgnoreCase) == true || vehicle.Durum?.Equals("Müsait", StringComparison.OrdinalIgnoreCase) == true))
                {
                    durumUyuyor = true;
                }
                else if (IsDoluSelected && (vehicle.Durum?.Equals("DOLU", StringComparison.OrdinalIgnoreCase) == true || vehicle.Durum?.Equals("Dolu", StringComparison.OrdinalIgnoreCase) == true))
                {
                    durumUyuyor = true;
                }
                else if (IsBakimdaSelected && (vehicle.Durum?.Equals("BAKIMDA", StringComparison.OrdinalIgnoreCase) == true || vehicle.Durum?.Equals("Bakımda", StringComparison.OrdinalIgnoreCase) == true))
                {
                    durumUyuyor = true;
                }

                if (!durumUyuyor) continue;

                var vSegment = string.IsNullOrWhiteSpace(vehicle.Segment) ? "Diğer" : vehicle.Segment;
                if (seciliSegmentler.Count > 0 && !seciliSegmentler.Contains(vSegment))
                {
                    continue; 
                }

                var vMarka = string.IsNullOrWhiteSpace(vehicle.Marka) ? "Diğer" : vehicle.Marka;
                if (seciliMarkalar.Count > 0 && !seciliMarkalar.Contains(vMarka))
                {
                    continue; 
                }

                AracListesi.Add(vehicle);
            }
        }

        /// <summary>Aktif sözleşmeyi uzatır.</summary>
        public async Task<(bool Success, string Message)> UzatSozlesme(Vehicle vehicle, int gunSayisi)
        {
            if (vehicle == null) return (false, "Ara\u00e7 bilgisi bo\u015f.");
            if (gunSayisi <= 0)  return (false, "G\u00fcn say\u0131s\u0131 s\u0131f\u0131rdan b\u00fcy\u00fck olmal\u0131d\u0131r.");
            if (gunSayisi > 30)  return (false, "Tek seferde en fazla 30 g\u00fcn uzatma yapabilirsiniz.");

            if (vehicle.ActiveRentalId == null)
            {
                var rentals = await _apiService.GetRentalsAsync();
                var aktif = rentals.FirstOrDefault(r =>
                    r.VehiclePlate == vehicle.Plaka &&
                    r.Status == "Aktif");
                if (aktif == null) return (false, "Bu ara\u00e7 i\u00e7in aktif kiralama bulunamad\u0131.");
                vehicle.ActiveRentalId = aktif.RentalId;
            }

            var (success, message, newEndDate) = await _apiService.ExtendRentalAsync(vehicle.ActiveRentalId!.Value, gunSayisi);

            if (success)
            {
                vehicle.uzatilanGunSayisi += gunSayisi;
                if (!string.IsNullOrEmpty(newEndDate))
                    vehicle.KiralamaSuresi = newEndDate;
                vehicle.TetikleBitisTarihiGuncellemesi();
            }

            return (success, message);
        }

        public async Task<(bool Success, string Message)> EkleRezervasyon(
            Vehicle vehicle,
            string musteriAdi,
            string musteriTelefon,
            DateTime baslangicTarihi,
            int gunSuresi)
        {
            if (vehicle == null) return (false, "Araç bilgisi boş.");
            if (string.IsNullOrWhiteSpace(musteriAdi)) return (false, "Geçerli bir müşteri adı giriniz.");
            if (gunSuresi <= 0)  return (false, "Süre sıfırdan büyük olmalıdır.");

            string firstName = musteriAdi;
            string lastName = "Misafir";
            var parts = musteriAdi.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                lastName = parts[parts.Length - 1];
                firstName = string.Join(" ", parts.Take(parts.Length - 1));
            }

            var bitisTarihi = baslangicTarihi.AddDays(gunSuresi);
            var (success, message) = await _apiService.CreateRentalWithGuestAsync(
                firstName,
                lastName,
                musteriTelefon,
                vehicle.Id,
                baslangicTarihi,
                bitisTarihi,
                depositAmount: 0,
                notes: "İleri tarihli rezervasyon.");

            if (success)
            {
                await LoadVehiclesFromApi(forceRefresh: true);
                
                vehicle.HasFutureReservation = true;
                vehicle.TetikleBitisTarihiGuncellemesi();
            }

            return (success, message);
        }
    }
}