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
            catch (Exception ex)
            {
                // HATA MESAJINI EKRANDA GÖSTER - sebebi anlayalım!
                var hataMesaji = ex.InnerException?.Message ?? ex.Message;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AracListesi.Clear();
                    AracListesi.Add(new Vehicle
                    {
                        Id = -1,
                        Plaka = "BAĞLANTI HATASI",
                        Marka = hataMesaji,  // <-- Asıl hata buraya yazılacak
                        Model = ex.GetType().Name,
                        Hp = 0,
                        Yas = 0,
                        Km = 0,
                        Durum = "BAKIMDA",
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