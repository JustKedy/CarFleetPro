namespace CarFleetPro.API.DTOs
{
    // Madde 6 — Hasar DTO'ları
    public class CreateDamageDto
    {
        public int VehicleId { get; set; }
        public string DamageType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDamageStatusDto
    {
        public Models.DamageStatus Status { get; set; }
    }

    public class DamageListDto
    {
        public int DamageId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string DamageType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
