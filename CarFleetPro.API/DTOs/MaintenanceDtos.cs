namespace CarFleetPro.API.DTOs
{
    public class CreateMaintenanceDto
    {
        public int VehicleId { get; set; }

        /// <summary>Periodic, Breakdown, Inspection, Other</summary>
        public string MaintenanceType { get; set; } = "Periodic";

        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal Cost { get; set; }
    }

    public class UpdateMaintenanceDto
    {
        /// <summary>Planned, InProgress, Done</summary>
        public string Status { get; set; } = "Planned";
        public DateTime? EndDate { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? NextInspectionDate { get; set; }
    }

    public class MaintenanceDto
    {
        public int MaintenanceId { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
