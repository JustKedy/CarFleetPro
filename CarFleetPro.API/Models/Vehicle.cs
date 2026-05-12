namespace CarFleetPro.API.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty; 
        public int BrandId { get; set; }
        public CarBrand Brand { get; set; } = null!;

        public int ModelId { get; set; }
        public CarModel Model { get; set; } = null!;

        public int? ColorId { get; set; }
        public CarColor? Color { get; set; }

        public int TypeId { get; set; }
        public CarType Type { get; set; } = null!;

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
        public decimal BasePrice { get; set; } = 0; // Specific price override
        public double MaxDiscountPercentage { get; set; } = 0; // Specific discount override
        public string Branch { get; set; } = "Merkez Şube"; 
    }
}
