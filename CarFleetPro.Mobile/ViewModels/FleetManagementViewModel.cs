using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private List<Vehicle> _tumAraclar = new List<Vehicle>();

        public ObservableCollection<Vehicle> AracListesi { get; set; } = new ObservableCollection<Vehicle>();

        public FleetManagementViewModel()
        {
            _apiService = new ApiService();
            _ = VerileriYukle();
        }

        private async Task VerileriYukle(bool forceRefresh = false)
        {
            var gelenAraclar = await _apiService.GetVehiclesAsync(forceRefresh);
            if (gelenAraclar != null)
            {
                _tumAraclar = gelenAraclar;
                Filtrele("Tümü");
            }
        }

        [RelayCommand]
        public async Task VerileriYenile()
        {
            await VerileriYukle(forceRefresh: true);
        }

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
                var filtrelenmis = _tumAraclar.Where(a => string.Equals(a.Durum, durum, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var arac in filtrelenmis) AracListesi.Add(arac);
            }
        }

        [RelayCommand]
        public async Task Duzenle(Vehicle secilenArac)
        {
            if (secilenArac == null || Shell.Current == null) return;
            // .NET 10 uyarılarını susturmak için Shell.Current kullanıyoruz
            await Shell.Current.DisplayAlert("Düzenle", $"{secilenArac.Marka} {secilenArac.Model} düzenleme sayfasına gidilecek.", "Tamam");
        }

        [RelayCommand]
        public async Task Sil(Vehicle secilenArac)
        {
            if (secilenArac == null || Shell.Current == null) return;

            bool cevap = await Shell.Current.DisplayAlert("Emin Misin?", $"{secilenArac.Plaka} plakalı aracı silmek istediğine emin misin?", "Evet, Sil", "İptal");

            if (cevap)
            {
                _tumAraclar.Remove(secilenArac);
                AracListesi.Remove(secilenArac);
            } 
        }
    }
}