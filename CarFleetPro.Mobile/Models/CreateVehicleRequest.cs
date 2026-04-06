namespace CarFleetPro.Mobile.Models
{
    /// <summary>
    /// POST /api/Vehicle endpoint'ine gönderilecek araç oluşturma isteği.
    /// API tarafındaki CreateVehicleDto ile birebir eşleşmeli.
    /// </summary>
    public class CreateVehicleRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int VehicleType { get; set; } = 0;       // 0 = Sedan
        public int FuelType { get; set; } = 0;          // 0 = Benzin
        public int TransmissionType { get; set; } = 0;  // 0 = Manuel
        public decimal DailyRate { get; set; } = 0;
        public int Mileage { get; set; }
        public int HorsePower { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public string Branch { get; set; } = "Merkez Şube";
        public int Status { get; set; } = 0; // 0=Müsait, 1=Kirada(Dolu), 2=Bakımda
    }
}
