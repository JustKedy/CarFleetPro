using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.ViewModels
{
    public partial class FleetManagementViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Vehicle> _tumAraclar = new();

        public ObservableCollection<Vehicle> AracListesi { get; set; } = new();

        
        [ObservableProperty]
        public partial bool IsLoading { get; set; } = true;

        
        [ObservableProperty]
        public partial string SeciliFiltre { get; set; } = "Tümü";

        [ObservableProperty]
        public partial bool IsAdmin { get; set; } = false;

        public FleetManagementViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            var profile = await _apiService.GetProfileAsync();
            var role = profile?.Role ?? string.Empty;
            IsAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                   || role.Equals("Yönetici", StringComparison.OrdinalIgnoreCase)
                   || role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            await VerileriYukle();
        }

        private async Task VerileriYukle(bool forceRefresh = false)
        {
            IsLoading = true;
            try
            {
                var gelenAraclar = await _apiService.GetVehiclesAsync(forceRefresh);
                if (gelenAraclar != null)
                {
                    _tumAraclar = gelenAraclar;
                    Filtrele("Tümü");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task VerileriYenile() => await VerileriYukle(forceRefresh: true);

        [RelayCommand]
        public void Filtrele(string durum)
        {
            
            SeciliFiltre = durum;

            AracListesi.Clear();

            if (string.Equals(durum, "Tümü", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var arac in _tumAraclar) AracListesi.Add(arac);
            }
            else
            {
                var filtrelenmis = _tumAraclar
                    .Where(a => string.Equals(a.Durum, durum, StringComparison.OrdinalIgnoreCase));
                foreach (var arac in filtrelenmis) AracListesi.Add(arac);
            }
        }

        [RelayCommand]
        public async Task Duzenle(Vehicle? secilenArac)
        {
            if (secilenArac is not null && Application.Current?.Windows.Count > 0)
            {
                
                await Application.Current.Windows[0].Page!.Navigation.PushAsync(new Views.AddNewVehiclePage(secilenArac));
            }
        }

        
        
        
        [RelayCommand]
        public async Task Sil(Vehicle? secilenArac)
        {
            if (secilenArac is not null && Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page!;
                bool cevap = await page.DisplayAlertAsync("Emin Misin?", $"{secilenArac.Plaka} plakalı aracı kalıcı olarak silmek istediğine emin misin?", "Evet, Sil", "İptal");

                if (cevap)
                {
                    var (apiBasarili, mesaj) = await _apiService.DeleteVehicleAsync(secilenArac.Id);

                    if (apiBasarili)
                    {
                        _tumAraclar.Remove(secilenArac);
                        AracListesi.Remove(secilenArac);
                        await page.DisplayAlertAsync("Başarılı", "Araç başarıyla filodan silindi.", "Tamam");
                    }
                    else
                    {
                        await page.DisplayAlertAsync("Hata", mesaj, "Tamam");
                    }
                }
            }
        }

        /// <summary>
        /// CommandParameter formatı: "{vehicleId}|{yeniDurum}"  örn: "25|MÜSAİT"
        /// </summary>
        [RelayCommand]
        public async Task DurumDegistir(string? param)
        {
            if (string.IsNullOrEmpty(param)) return;

            var parts = param.Split('|');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int vehicleId)) return;

            var yeniDurum = parts[1]; // MÜSAİT | DOLU | BAKIMDA

            if (Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page!;
                var (basarili, mesaj) = await _apiService.UpdateVehicleStatusAsync(vehicleId, yeniDurum);

                if (basarili)
                {
                    // Listedeki aracın durumunu anında güncelle (yeniden yüklemeye gerek kalmadan)
                    var arac = _tumAraclar.FirstOrDefault(a => a.Id == vehicleId);
                    if (arac != null)
                    {
                        arac.Durum = yeniDurum;
                        await VerileriYukle(forceRefresh: true);
                    }
                }
                else
                {
                    await page.DisplayAlertAsync("Hata", mesaj, "Tamam");
                }
            }
        }
    }
}
