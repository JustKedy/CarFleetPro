using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;

namespace CarFleetPro.Mobile.Models
{
    public partial class Vehicle : ObservableObject
    {
        public int Id { get; set; }
        public string Plaka { get; set; } = string.Empty;
        public string Marka { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Yas { get; set; }
        public int Km { get; set; }
        public string Durum { get; set; } = string.Empty;
        public string? KiralayanKisi { get; set; }
        public decimal? KiralamaFiyati { get; set; }
        public string? KiralamaSuresi { get; set; }
        public string? KiralamaTarihi { get; set; }
        public string? ResimUrl { get; set; }
        public string? Branch { get; set; }
        public decimal GunlukUcret { get; set; }
        public string Segment { get; set; } = "Ekonomik";
        public decimal BasePrice { get; set; }
        public double MaxDiscountPercentage { get; set; }

        // Rezervasyon desteği
        /// <summary>İleri tarihli rezervasyon var mı? Garaj kartında "Rezervasyonlu" badge göstermek için.</summary>
        public bool HasFutureReservation { get; set; }
        /// <summary>Aktif kiralamanın ID'si — sözleşme uzatma API çağrısı için gerekli.</summary>
        public int? ActiveRentalId { get; set; }
        /// <summary>Bu araç için dolu tarih aralıkları (kiralama takvimi kırmızı tarihleri).</summary>
        public List<OccupiedDateRange> OccupiedDates { get; set; } = new();

        public string DisplayName => $"{Marka} {Model} ({Plaka})";

        /// <summary>0=Müsait, 1=Kirada(Dolu), 2=Bakımda</summary>

        public int StatusCode
        {
            get
            {
                if (Durum == null) return 0;
                var d = Durum.ToUpperInvariant().Trim();
                if (d.Contains("DOLU")   || d.Contains("KIRAD") || d.Contains("RENTED"))   return 1;
                if (d.Contains("BAKIM") || d.Contains("MAINTENANCE"))                       return 2;
                return 0; // MÜSAİT / AVAILABLE
            }
        }


        [ObservableProperty]
        public partial bool IsExpanded { get; set; }

        [ObservableProperty]
        public partial string? KiralayanTelefon { get; set; }

        [ObservableProperty]
        public partial string? KiralayanTc { get; set; }

        [ObservableProperty]
        public partial string? KiralayanEhliyetNo { get; set; }

        [ObservableProperty]
        public partial string? KiralayanEhliyetGecerlilik { get; set; }

        [ObservableProperty]
        public partial string? KiralayanAdres { get; set; }

        [ObservableProperty]
        public partial string? KiralayanNotlar { get; set; }

        private System.Collections.ObjectModel.ObservableCollection<RentalInfo> _rezervasyonlar = new();
        public System.Collections.ObjectModel.ObservableCollection<RentalInfo> Rezervasyonlar
        {
            get => _rezervasyonlar;
            set => SetProperty(ref _rezervasyonlar, value);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UzatmaLimitineUlasildi))]
        [NotifyPropertyChangedFor(nameof(RezervasyonBaslangicMinTarihi))]
        [NotifyPropertyChangedFor(nameof(MaksimumUzatilabilirGunSayisi))]
        public partial int uzatilanGunSayisi { get; set; } = 0;

        public bool UzatmaLimitineUlasildi => uzatilanGunSayisi >= 30; // Sektörel limit: 30 Gün.

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UzatSecilenGunSayisi))]
        public partial string UzatSecilenGunMetni { get; set; } = "1";

        public int UzatSecilenGunSayisi
        {
            get
            {
                if (int.TryParse(UzatSecilenGunMetni, out int val) && val > 0) return val;
                return 1;
            }
        }

        public int KiralamaGunSayisi
        {
            get
            {
                if (DateTime.TryParseExact(KiralamaTarihi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var baslangic) &&
                    DateTime.TryParseExact(KiralamaSuresi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var bitis))
                {
                    return Math.Max((bitis.Date - baslangic.Date).Days, 0);
                }
                return 0;
            }
        }

        public string KiralamaOzetiMetni
        {
            get
            {
                if (string.IsNullOrEmpty(KiralamaTarihi) || string.IsNullOrEmpty(KiralamaSuresi))
                    return "-";
                return $"{KiralamaTarihi} - {KiralamaSuresi} ({KiralamaGunSayisi} Gün)";
            }
        }

        public int MaksimumUzatilabilirGunSayisi
        {
            get
            {
                int kalanLimit = 30 - uzatilanGunSayisi; // Sektörel olarak toplamda en fazla 30 gün uzatılabilir.
                if (kalanLimit < 0) kalanLimit = 0;

                if (Rezervasyonlar == null || Rezervasyonlar.Count == 0)
                {
                    return kalanLimit;
                }

                if (!DateTime.TryParseExact(KiralamaSuresi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var bitisDate))
                {
                    return kalanLimit;
                }

                // Aktif ve en yakın rezervasyonu bulalım
                var enYakinRez = Rezervasyonlar
                    .Where(r => r.Status != null && r.Status.Equals("Aktif", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.StartDate)
                    .FirstOrDefault();

                if (enYakinRez != null)
                {
                    int gunFarki = (enYakinRez.StartDate.Date - bitisDate.Date).Days;
                    int limitByRez = gunFarki - 1;
                    if (limitByRez < 0) limitByRez = 0;
                    return Math.Min(kalanLimit, limitByRez); // En yakın rezervasyon başlangıç tarihinin 1 gün öncesine kadar kısıtlanmıştır
                }

                return kalanLimit;
            }
        }

        public string RezervasyonBaslangicMinTarihi
        {
            get
            {
                if (!string.IsNullOrEmpty(KiralamaSuresi) && DateTime.TryParseExact(KiralamaSuresi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var bitisDate))
                {
                    // 5 günlük koruma/opsiyon süresi + 1 gün = en erken 6 gün sonra yeni rezervasyon başlayabilir
                    return bitisDate.AddDays(6).ToString("yyyy-MM-dd");
                }
                return DateTime.Today.AddDays(6).ToString("yyyy-MM-dd");
            }
        }

        [ObservableProperty]
        public partial bool IsUzatmaVisible { get; set; } = false;

        [ObservableProperty]
        public partial bool IsRezervasyonVisible { get; set; } = false;

        [ObservableProperty]
        public partial string? RezMusteriAdi { get; set; }

        [ObservableProperty]
        public partial string? RezMusteriTelefon { get; set; }

        [ObservableProperty]
        public partial string RezGunSuresi { get; set; } = "1";

        [ObservableProperty]
        public partial DateTime RezBaslangicTarihi { get; set; } = DateTime.Today.AddDays(1);

        [ObservableProperty]
        public partial int UzatSecilenGunIndex { get; set; } = 0;

        public void TetikleBitisTarihiGuncellemesi()
        {
            OnPropertyChanged(nameof(KiralamaSuresi));
            OnPropertyChanged(nameof(KiralamaGunSayisi));
            OnPropertyChanged(nameof(KiralamaOzetiMetni));
            OnPropertyChanged(nameof(RezervasyonBaslangicMinTarihi));
            OnPropertyChanged(nameof(MaksimumUzatilabilirGunSayisi));
            
            // Rezervasyon başlangıç tarihini de minimum tarihe göre güncelle
            if (DateTime.TryParseExact(RezervasyonBaslangicMinTarihi, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var minDate))
            {
                if (RezBaslangicTarihi < minDate)
                {
                    RezBaslangicTarihi = minDate;
                    OnPropertyChanged(nameof(RezBaslangicTarihi));
                }
            }
        }
    }
}