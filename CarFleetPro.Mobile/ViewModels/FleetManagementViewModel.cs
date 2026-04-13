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
        [ObservableProperty]
        public partial bool IsLoading { get; set; } = true;

        // Seçili filtrenin rengini XAML tarafında değiştirmek için hafıza
        [ObservableProperty]
        public partial string SeciliFiltre { get; set; } = "Tümü";

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
            // Hangi butona basıldıysa hafızaya alıyoruz ki arayüz bilsin
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
                // Kadir'in hazırladığı arayüze (Düzenleme Moduyla) geçiş yapıyoruz
                await Application.Current.Windows[0].Page!.Navigation.PushAsync(new Views.AddNewVehiclePage(secilenArac));
            }
        }

        // =========================================================================
        // [GÜNCELLENDİ] API BAĞLANTILI SİLME İŞLEMİ
        // =========================================================================
        [RelayCommand]
        public async Task Sil(Vehicle? secilenArac)
        {
            if (secilenArac is not null && Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page!;
                bool cevap = await page.DisplayAlertAsync("Emin Misin?", $"{secilenArac.Plaka} plakalı aracı kalıcı olarak silmek istediğine emin misin?", "Evet, Sil", "İptal");

                if (cevap)
                {
                    // 1. API üzerinden Alper'in veritabanından siliyoruz (Id propertysini kullanıyoruz)
                    bool apiBasarili = await _apiService.DeleteVehicleAsync(secilenArac.Id);

                    if (apiBasarili)
                    {
                        // 2. Veritabanından başarıyla silindiyse, ekrandaki listeden de uçur!
                        _tumAraclar.Remove(secilenArac);
                        AracListesi.Remove(secilenArac);
                        
                        await page.DisplayAlertAsync("Başarılı", "Araç başarıyla filodan silindi.", "Tamam");
                    }
                    else
                    {
                        // Silerken hata çıkarsa kullanıcıyı uyar
                        await page.DisplayAlertAsync("Hata", "Araç silinirken veritabanı tarafında bir sorun oluştu.", "Tamam");
                    }
                }
            }
        }
    }
}