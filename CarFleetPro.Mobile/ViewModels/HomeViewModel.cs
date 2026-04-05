using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Vehicle> AracListesi { get; set; }

        public HomeViewModel()
        {
            _apiService = new ApiService();
            AracListesi = new ObservableCollection<Vehicle>();

            // Sayfa açıldığında verileri çekme işlemini başlat
            _ = LoadVehiclesFromApi();
        }

        private async Task LoadVehiclesFromApi()
        {
            try
            {
                // 1. API'ye bağlanıp veriyi çekiyoruz
                var apiVehicles = await _apiService.GetVehiclesAsync();

                // 2. Eğer Alper'in veritabanı boşsa, hata fırlatıp aşağıdaki catch bloğuna düşürüyoruz
                if (apiVehicles == null || apiVehicles.Count == 0)
                {
                    throw new Exception("API boş liste döndürdü.");
                }

                // 3. Veritabanında gerçekten araç varsa ekrana basıyoruz
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AracListesi.Clear();
                    foreach (var vehicle in apiVehicles) { AracListesi.Add(vehicle); }
                });
            }
            catch
            {
                // API KAPALIYSA VEYA VERİTABANI BOŞSA BURASI KESİN ÇALIŞACAK
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AracListesi.Clear();
                    AracListesi.Add(new Vehicle
                    {
                        Id = Guid.NewGuid(),
                        Plaka = "34 HATA 404",
                        Marka = "VERİTABANI BOŞ",
                        Model = "VEYA API KAPALI",
                        Hp = 150,
                        Yas = 2,
                        Km = 25000,
                        Durum = "MÜSAİT",
                        KiralamaFiyati = 30000,
                        KiralamaSuresi = "5 Gün",
                        IsExpanded = false
                    });
                });
            }
        }

        // Akordeon (Aşağı Ok) animasyonu için komut
        [RelayCommand]
        public void DetayAcKapa(Vehicle selectedVehicle)
        {
            if (selectedVehicle != null)
            {
                selectedVehicle.IsExpanded = !selectedVehicle.IsExpanded;
            }
        }
    }
}