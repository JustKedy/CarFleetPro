using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarFleetPro.Mobile.ViewModels
{
    // Yunus gelip içini doldurana kadar hata vermemesi için oluşturduğumuz boş motor iskeleti.
    // XAML tarafında "FallbackValue" kullandığımız için veriler yine de ekranda görünecek!
    public partial class HomeViewModel : ObservableObject
    {
        private readonly Services.ApiService _apiService = new();

        [ObservableProperty] private int toplamAracSayisi;
        [ObservableProperty] private int kiradakiAracSayisi;
        [ObservableProperty] private int musaitAracSayisi;

        [ObservableProperty] private string kiraYuzdesi = "0";
        [ObservableProperty] private string musaitYuzdesi = "0";
        [ObservableProperty] private string bakimYuzdesi = "0";

        [ObservableProperty] private string grafikOranlari = "1*, 1*, 1*";

        public HomeViewModel()
        {
            _ = LoadDataAsync();
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            var vehicles = await _apiService.GetVehiclesAsync();
            
            int total = vehicles.Count;
            if (total == 0) return; // Boşsa fallback değerler kalsın veya 0'la

            ToplamAracSayisi = total;
            MusaitAracSayisi = vehicles.Count(v => v.Durum == "MÜSAİT");
            KiradakiAracSayisi = vehicles.Count(v => v.Durum == "KİRADA" || v.Durum == "DOLU");
            int bakimdaSayisi = vehicles.Count(v => v.Durum == "BAKIMDA");

            KiraYuzdesi = ((KiradakiAracSayisi * 100) / total).ToString();
            MusaitYuzdesi = ((MusaitAracSayisi * 100) / total).ToString();
            BakimYuzdesi = ((bakimdaSayisi * 100) / total).ToString();

            // Grid column ratio örneği: "5*, 3*, 1*"
            GrafikOranlari = $"{KiradakiAracSayisi}*, {MusaitAracSayisi}*, {bakimdaSayisi}*";
            if (GrafikOranlari == "0*, 0*, 0*") GrafikOranlari = "1*, 1*, 1*"; // Sıfıra bölünmeyi engellemek için
        }
    }
}