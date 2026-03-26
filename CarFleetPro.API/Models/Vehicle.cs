namespace CarFleetPro.API.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty; // Plaka (Benzersiz olacak)
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public VehicleType VehicleType { get; set; }
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public decimal DailyRate { get; set; }
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public int Mileage { get; set; } // Kilometre
        public DateTime InsuranceExpiry { get; set; } // Sigorta Bitiş
        public DateTime InspectionExpiry { get; set; } // Muayene Bitiş
        public string? PhotoUrl { get; set; } // Cloudinary'den gelecek link
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
