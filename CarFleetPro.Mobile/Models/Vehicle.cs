using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CarFleetPro.Mobile.Models
{
    public partial class Vehicle : ObservableObject
    {
        public Guid Id { get; set; }
        public string Plaka { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public int Hp { get; set; }
        public int Yas { get; set; }
        public int Km { get; set; }
        public string Durum { get; set; } // "MÜSAİT", "DOLU", "BAKIMDA"
        public string? KiralayanKisi { get; set; }
        public decimal KiralamaFiyati { get; set; }
        public string KiralamaSuresi { get; set; }
        public DateTime? KiralamaTarihi { get; set; }
        public string ResimUrl { get; set; }

        // Kadir'in Akordeon yapısı için gereken UI tetikleyicisi
        [ObservableProperty]
        private bool _isExpanded;
    }
}