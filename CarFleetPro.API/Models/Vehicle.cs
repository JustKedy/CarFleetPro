namespace CarFleetPro.API.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty; 
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public VehicleType VehicleType { get; set; }
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public decimal DailyRate { get; set; }
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public int Mileage { get; set; } 
        public DateTime InsuranceExpiry { get; set; } 
        public DateTime InspectionExpiry { get; set; } 
        public string? PhotoUrl { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int HorsePower { get; set; }
        public string? ImageUrl { get; set; } 
        public string? Color { get; set; }
        public string Branch { get; set; } = "Merkez Şube"; 
    }
}
