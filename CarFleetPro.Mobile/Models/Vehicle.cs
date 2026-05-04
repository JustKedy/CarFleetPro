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
        public decimal DailyRate { get; set; }
        public string? KiralayanKisi { get; set; }
        public decimal? KiralamaFiyati { get; set; }
        public string? KiralamaSuresi { get; set; }
        public string? KiralamaTarihi { get; set; }  
        public string? ResimUrl { get; set; }

        
        [ObservableProperty]
        public partial bool IsExpanded { get; set; }
    }
}