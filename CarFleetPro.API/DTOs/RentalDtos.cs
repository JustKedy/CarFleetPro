namespace CarFleetPro.API.DTOs
{
    public class CreateRentalDto
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public decimal DepositAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CompleteRentalDto
    {
        public DateTime ActualEndDate { get; set; }
        public int EndMileage { get; set; }
    }

    public class RentalListDto
    {
        public int RentalId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}