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

        // AOT UYARISI ÇÖZÜMÜ
        [ObservableProperty] public partial bool IsLoading { get; set; } = true;

        public FleetManagementViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = VerileriYukle();
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

        // STATİC UYARISI VE NULL CHECK ÇÖZÜMÜ
        [RelayCommand]
        public static async Task Duzenle(Vehicle? secilenArac)
        {
            if (secilenArac is not null && Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync("Düzenle", $"{secilenArac.Marka} {secilenArac.Model} düzenleme sayfasına gidilecek.", "Tamam");
            }
        }

        // NULL CHECK ÇÖZÜMÜ
        [RelayCommand]
        public async Task Sil(Vehicle? secilenArac)
        {
            if (secilenArac is not null && Shell.Current is not null)
            {
                bool cevap = await Shell.Current.DisplayAlertAsync("Emin Misin?", $"{secilenArac.Plaka} plakalı aracı silmek istediğine emin misin?", "Evet, Sil", "İptal");

                if (cevap)
                {
                    _tumAraclar.Remove(secilenArac);
                    AracListesi.Remove(secilenArac);
                }
            }
        }
    }
}