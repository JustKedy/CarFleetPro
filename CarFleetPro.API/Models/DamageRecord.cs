namespace CarFleetPro.API.Models
{
    public class DamageRecord
    {
        public int DamageRecordId { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        // Hangi çalışan kaydetti
        public string? ReportedByUserId { get; set; }
        public AppUser? ReportedByUser { get; set; }

        public DamageType DamageType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DamageDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DamageRecordStatus Status { get; set; } = DamageRecordStatus.Pending;

        // Gelecekte fotoğraf URL'leri için
        public string? PhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
