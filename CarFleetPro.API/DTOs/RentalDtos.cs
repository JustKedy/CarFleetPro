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
}