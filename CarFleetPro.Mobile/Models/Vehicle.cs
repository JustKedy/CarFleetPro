using CommunityToolkit.Mvvm.ComponentModel;

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
    }
}