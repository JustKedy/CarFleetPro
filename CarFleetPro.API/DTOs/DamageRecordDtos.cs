namespace CarFleetPro.API.DTOs
{
    public class CreateDamageRecordDto
    {
        public int VehicleId { get; set; }

        /// <summary>Body, Mechanical, Glass, Interior, Other</summary>
        public string DamageType { get; set; } = "Other";

        public string Description { get; set; } = string.Empty;
        public DateTime DamageDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class UpdateDamageRecordDto
    {
        /// <summary>Pending, UnderRepair, Completed</summary>
        public string Status { get; set; } = "Pending";
        public decimal? EstimatedCost { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class DamageRecordDto
    {
        public int DamageRecordId { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string DamageType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DamageDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? ReportedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
