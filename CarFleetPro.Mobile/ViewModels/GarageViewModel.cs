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

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _tumAraclar.Clear();
                    _tumAraclar.AddRange(apiVehicles);

                    // Türleri (Segment) doldur
                    var mevcutSegmentler = SegmentFilters.Select(s => s.Name).ToHashSet();
                    foreach (var seg in apiSegmentler)
                    {
                        if (!mevcutSegmentler.Contains(seg.Name))
                        {
                            SegmentFilters.Add(new SegmentFilterItem { Name = seg.Name, IsSelected = false });
                        }
                    }

                    // Markaları doldur
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
                // Önce Durum filtresi (Tümü, Müsait vs.)
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

                // Segment filtresi
                var vSegment = string.IsNullOrWhiteSpace(vehicle.Segment) ? "Diğer" : vehicle.Segment;
                if (seciliSegmentler.Count > 0 && !seciliSegmentler.Contains(vSegment))
                {
                    continue; 
                }

                // Marka filtresi
                var vMarka = string.IsNullOrWhiteSpace(vehicle.Marka) ? "Diğer" : vehicle.Marka;
                if (seciliMarkalar.Count > 0 && !seciliMarkalar.Contains(vMarka))
                {
                    continue; 
                }

                AracListesi.Add(vehicle);
            }
        }
    }
}