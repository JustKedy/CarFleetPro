using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.ViewModels
{
    // Derlenmiş bağlamalar (x:DataType) için bu namespace'in XAML tarafında tanımlı olması gerekir.
    // ViewModel artık DI üzerinden ApiService alıyor (new() yerine constructor injection).
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        // ─── Dashboard İstatistikleri ──────────────────────────────────
        [ObservableProperty] private int toplamAracSayisi;
        [ObservableProperty] private int kiradakiAracSayisi;
        [ObservableProperty] private int musaitAracSayisi;

        [ObservableProperty] private string kiraYuzdesi  = "0";
        [ObservableProperty] private string musaitYuzdesi = "0";
        [ObservableProperty] private string bakimYuzdesi  = "0";

        [ObservableProperty] private ColumnDefinitionCollection grafikOranlari = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        };
        [ObservableProperty] private int    aylikCiro;
        [ObservableProperty] private string aracModelAdi     = string.Empty;
        [ObservableProperty] private double barGenisligi     = 0;
        [ObservableProperty] private int    kiralamaSayisi;

        // ─── Skeleton Screen Kontrolü ──────────────────────────────────
        // IsLoading=true  → Skeleton görünür, gerçek içerik gizlenir
        // IsLoading=false → Skeleton gizlenir, gerçek içerik görünür
        [ObservableProperty] private bool isLoading = true;

        // ─── Constructor (DI) ─────────────────────────────────────────
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
                var vehicles = await _apiService.GetVehiclesAsync();

                if (vehicles == null || vehicles.Count == 0) return; // Boşsa fallback değerler kalsın

                int total = vehicles.Count;
                ToplamAracSayisi    = total;
                MusaitAracSayisi    = vehicles.Count(v => v.Durum == "MÜSAİT");
                KiradakiAracSayisi  = vehicles.Count(v => v.Durum == "KİRADA" || v.Durum == "DOLU");
                int bakimdaSayisi   = vehicles.Count(v => v.Durum == "BAKIMDA");

                KiraYuzdesi   = ((KiradakiAracSayisi * 100) / total).ToString();
                MusaitYuzdesi = ((MusaitAracSayisi   * 100) / total).ToString();
                BakimYuzdesi  = ((bakimdaSayisi      * 100) / total).ToString();

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
                // Başarılı ya da hatalı her durumda skeleton'ı kaldır
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void SubeFiltrele(string subeId) { /* Şube filtresi ileride API'ye taşınacak */ }
    }
}