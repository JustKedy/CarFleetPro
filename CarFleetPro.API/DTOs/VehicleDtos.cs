namespace CarFleetPro.API.DTOs
{
    public class VehicleCardDto
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
    }

    public class CreateVehicleDto
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public Models.VehicleType VehicleType { get; set; }
        public Models.FuelType FuelType { get; set; }
        public Models.TransmissionType TransmissionType { get; set; }
        public decimal DailyRate { get; set; }
        public int Mileage { get; set; }
        public int HorsePower { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public string? Branch { get; set; }
        public Models.VehicleStatus Status { get; set; }
    }

    /// <summary>
    /// Araç detay sayfası için kapsamlı DTO (bakım + kiralama geçmişi dahil)
    /// </summary>
    public class VehicleDetailDto
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string TransmissionType { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = string.Empty; // "MÜSAİT", "DOLU", "BAKIMDA"
        public int Mileage { get; set; }
        public int HorsePower { get; set; }
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public string Branch { get; set; } = string.Empty;
        public List<VehicleHistoryItemDto> History { get; set; } = new();
    }

    public class VehicleHistoryItemDto
    {
        public string Type { get; set; } = string.Empty; // "Kiralama" veya "Bakım"
        public string Title { get; set; } = string.Empty; // "Kiralandı: Abdulkadir Toksöz" veya "Periyodik Bakım"
        public string DateRange { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Tamamlandı", "Aktif", "Devam Ediyor"
        public string? Amount { get; set; } // "12.000 TL"
        public string Color { get; set; } = "#3B82F6"; // Timeline rengi
    }
}