using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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

                if (apiVehicles == null || apiVehicles.Count == 0)
                    throw new Exception("API boş liste döndürdü.");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _tumAraclar.Clear();
                    _tumAraclar.AddRange(apiVehicles);
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
        public static void DetayAcKapa(Vehicle? selectedVehicle)
        {
            if (selectedVehicle is null) return;
            selectedVehicle.IsExpanded = !selectedVehicle.IsExpanded;
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
            foreach (var vehicle in _tumAraclar)
            {
                if (IsTumuSelected)
                {
                    AracListesi.Add(vehicle);
                }
                else if (IsMusaitSelected && (vehicle.Durum?.Equals("MÜSAİT", StringComparison.OrdinalIgnoreCase) == true || vehicle.Durum?.Equals("Müsait", StringComparison.OrdinalIgnoreCase) == true))
                {
                    AracListesi.Add(vehicle);
                }
                else if (IsDoluSelected && (vehicle.Durum?.Equals("DOLU", StringComparison.OrdinalIgnoreCase) == true || vehicle.Durum?.Equals("Dolu", StringComparison.OrdinalIgnoreCase) == true))
                {
                    AracListesi.Add(vehicle);
                }
                else if (IsBakimdaSelected && (vehicle.Durum?.Equals("BAKIMDA", StringComparison.OrdinalIgnoreCase) == true || vehicle.Durum?.Equals("Bakımda", StringComparison.OrdinalIgnoreCase) == true))
                {
                    AracListesi.Add(vehicle);
                }
            }
        }
    }
}