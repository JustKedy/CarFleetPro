using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        // ── Kart istatistikleri ──────────────────────────────────────────────
        [ObservableProperty] public partial int ToplamAracSayisi { get; set; }
        [ObservableProperty] public partial int KiradakiAracSayisi { get; set; }
        [ObservableProperty] public partial int MusaitAracSayisi { get; set; }

        // ── Filo durum yüzdeleri ─────────────────────────────────────────────
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

        // ── Aylık ciro (sadece Admin görür) ─────────────────────────────────
        [ObservableProperty] public partial decimal AylikCiro { get; set; }
        [ObservableProperty] public partial bool IsAdmin { get; set; } = false;

        // ── En çok talep gören modeller ──────────────────────────────────────
        [ObservableProperty] public partial string AracModelAdi { get; set; } = "-";
        [ObservableProperty] public partial double BarGenisligi { get; set; } = 0;
        [ObservableProperty] public partial int KiralamaSayisi { get; set; }

        [ObservableProperty] public partial string AracModelAdi2 { get; set; } = "-";
        [ObservableProperty] public partial double BarGenisligi2 { get; set; } = 0;
        [ObservableProperty] public partial int KiralamaSayisi2 { get; set; }

        // ── Yükleme durumu ───────────────────────────────────────────────────
        [ObservableProperty] public partial bool IsLoading { get; set; } = true;

        public HomeViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            // Rol kontrolü
            var profile = await _apiService.GetProfileAsync();
            System.Diagnostics.Debug.WriteLine($"[HOME VM] Profil: {profile?.FullName}, Rol: '{profile?.Role}'");

            var role = profile?.Role ?? string.Empty;
            IsAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                   || role.Equals("Yönetici", StringComparison.OrdinalIgnoreCase)
                   || role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            System.Diagnostics.Debug.WriteLine($"[HOME VM] IsAdmin: {IsAdmin}");

            await LoadDataAsync();
        }


        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (ToplamAracSayisi == 0) IsLoading = true;
            try
            {
                var stats = await _apiService.GetDashboardStatsAsync();
                if (stats == null) return;

                ToplamAracSayisi = stats.TotalVehicles;
                KiradakiAracSayisi = stats.RentedVehicles;
                MusaitAracSayisi = stats.AvailableVehicles;
                AylikCiro = stats.MonthlyRevenue ?? 0m;

                KiraYuzdesi = ((int)Math.Round(stats.RentedPercentage)).ToString();
                MusaitYuzdesi = ((int)Math.Round(stats.AvailablePercentage)).ToString();
                BakimYuzdesi = ((int)Math.Round(stats.MaintenancePercentage)).ToString();

                var c1 = Math.Max(1, stats.RentedVehicles);
                var c2 = Math.Max(1, stats.AvailableVehicles);
                var c3 = Math.Max(1, stats.TotalVehicles - stats.RentedVehicles - stats.AvailableVehicles);

                GrafikOranlari = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(c1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(c2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(c3, GridUnitType.Star) }
                };

                // En çok talep gören modeller
                const double MaxBarWidth = 180.0;
                var topModels = stats.TopModels;

                int maxCount = topModels.Count > 0 ? topModels.Max(m => m.RentCount) : 1;
                if (maxCount == 0) maxCount = 1;

                if (topModels.Count > 0)
                {
                    AracModelAdi = topModels[0].ModelName;
                    KiralamaSayisi = topModels[0].RentCount;
                    BarGenisligi = (topModels[0].RentCount / (double)maxCount) * MaxBarWidth;
                }
                else
                {
                    AracModelAdi = "-";
                    KiralamaSayisi = 0;
                    BarGenisligi = 0;
                }

                if (topModels.Count > 1)
                {
                    AracModelAdi2 = topModels[1].ModelName;
                    KiralamaSayisi2 = topModels[1].RentCount;
                    BarGenisligi2 = (topModels[1].RentCount / (double)maxCount) * MaxBarWidth;
                }
                else
                {
                    AracModelAdi2 = "-";
                    KiralamaSayisi2 = 0;
                    BarGenisligi2 = 0;
                }
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
    }
}