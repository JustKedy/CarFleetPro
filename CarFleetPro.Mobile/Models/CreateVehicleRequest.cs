namespace CarFleetPro.Mobile.Models
{
    
    
    
    
    public class CreateVehicleRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public int ModelId { get; set; }
        public int Year { get; set; }
        public int VehicleType { get; set; } = 0;       
        public int FuelType { get; set; } = 0;          
        public int TransmissionType { get; set; } = 0;  
        public decimal DailyRate { get; set; } = 0;
        public int Mileage { get; set; }
        public int HorsePower { get; set; }
        public string? ImageUrl { get; set; }
        public int? ColorId { get; set; }
        public string Branch { get; set; } = "Merkez Şube";
        public int Status { get; set; } = 0; 
        public int SegmentId { get; set; }
        public decimal BasePrice { get; set; }
        public double MaxDiscountPercentage { get; set; }
    }
}
