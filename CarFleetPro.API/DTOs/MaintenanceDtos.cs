namespace CarFleetPro.API.DTOs
{
    // Madde 5 — Bakım DTO'ları
    public class CreateMaintenanceDto
    {
        public int VehicleId { get; set; }
        public string Description { get; set; } = string.Empty;
        public Models.MaintenanceType MaintenanceType { get; set; } = Models.MaintenanceType.Periyodik;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal Cost { get; set; }
    }

    public class MaintenanceListDto
    {
        public int MaintenanceId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
