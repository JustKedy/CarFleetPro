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
    }
}