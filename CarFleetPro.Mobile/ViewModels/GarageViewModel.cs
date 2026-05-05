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

        [ObservableProperty] public partial bool IsLoading { get; set; } = true;

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
                    AracListesi.Clear();
                    foreach (var vehicle in apiVehicles) { AracListesi.Add(vehicle); }
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
    }
}