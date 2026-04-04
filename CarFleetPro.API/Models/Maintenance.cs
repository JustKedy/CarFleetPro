namespace CarFleetPro.API.Models
{
    public class Maintenance
    {
        public int MaintenanceId { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Cost { get; set; }
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Planned;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
