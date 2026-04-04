namespace CarFleetPro.API.Models
{
    public class Rental
    {
        public int RentalId { get; set; }

        // İlişkiler (Foreign Keys)
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public string UserId { get; set; } = string.Empty; // Agent (Kullanıcı) ID'si
        public AppUser? User { get; set; }

        // Kiralama Detayları
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int StartMileage { get; set; }
        public int EndMileage { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public RentalStatus Status { get; set; } = RentalStatus.Active;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}