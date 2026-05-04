using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        
        [ObservableProperty] public partial int ToplamAracSayisi { get; set; }
        [ObservableProperty] public partial int KiradakiAracSayisi { get; set; }
        [ObservableProperty] public partial int MusaitAracSayisi { get; set; }

        [ObservableProperty] public partial string KiraYuzdesi { get; set; } = "0";
        [ObservableProperty] public partial string MusaitYuzdesi { get; set; } = "0";
        [ObservableProperty] public partial string BakimYuzdesi { get; set; } = "0";

        [ObservableProperty]
        public partial ColumnDefinitionCollection GrafikOranlari { get; set; } = new()
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        };

        [ObservableProperty] public partial int AylikCiro { get; set; }
        [ObservableProperty] public partial string AracModelAdi { get; set; } = string.Empty;
        [ObservableProperty] public partial double BarGenisligi { get; set; } = 0;
        [ObservableProperty] public partial int KiralamaSayisi { get; set; }

        [ObservableProperty] public partial bool IsLoading { get; set; } = true;
        [ObservableProperty] public partial bool IsAdmin { get; set; }

        public HomeViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (ToplamAracSayisi == 0) IsLoading = true;
            try
            {
                IsAdmin = await SessionManager.IsAdminAsync();
                var vehicles = await _apiService.GetVehiclesAsync();

                if (vehicles == null || vehicles.Count == 0) return;

                int total = vehicles.Count;
                ToplamAracSayisi = total;
                MusaitAracSayisi = vehicles.Count(v => v.Durum == "MÜSAİT");
                KiradakiAracSayisi = vehicles.Count(v => v.Durum == "KİRADA" || v.Durum == "DOLU");
                int bakimdaSayisi = vehicles.Count(v => v.Durum == "BAKIMDA");

                KiraYuzdesi = ((KiradakiAracSayisi * 100) / total).ToString();
                MusaitYuzdesi = ((MusaitAracSayisi * 100) / total).ToString();
                BakimYuzdesi = ((bakimdaSayisi * 100) / total).ToString();

                var c1 = Math.Max(1, KiradakiAracSayisi);
                var c2 = Math.Max(1, MusaitAracSayisi);
                var c3 = Math.Max(1, bakimdaSayisi);

                GrafikOranlari = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(c1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(c2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(c3, GridUnitType.Star) }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HOME VM] Yükleme Hatası: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void SubeFiltrele(string subeId) {  }
    }
}