using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.ViewModels
{
    // 1. SINIF ADI DEĞİŞTİ
    public partial class GarageViewModel : ObservableObject
    {
        private readonly ApiService _apiService = new();

        public ObservableCollection<Vehicle> AracListesi { get; set; } = [];

        // 2. CONSTRUCTOR (YAPICI METOT) ADI DEĞİŞTİ
        public GarageViewModel()
        {
            _ = LoadVehiclesFromApi();
        }

        private async Task LoadVehiclesFromApi(bool forceRefresh = false)
        {
            try
            {
                // 1. API'ye bağlanıp veriyi çekiyoruz
                var apiVehicles = await _apiService.GetVehiclesAsync(forceRefresh);

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

        [RelayCommand]
        public async Task VerileriYenile()
        {
            await LoadVehiclesFromApi(forceRefresh: true);
        }

        // Akordeon (Aşağı Ok) animasyonu için komut
        [RelayCommand]
        public static void DetayAcKapa(Vehicle? selectedVehicle)
        {
            // "Guard Clause" (Koruyucu Cümle) - Eğer null ise metottan hemen çık.
            // IDE0031 bu yapıyı çok sever çünkü kodun geri kalanı daha "düz" ve okunabilir olur.
            if (selectedVehicle is null) return;

            selectedVehicle.IsExpanded = !selectedVehicle.IsExpanded;
        }
    }
}