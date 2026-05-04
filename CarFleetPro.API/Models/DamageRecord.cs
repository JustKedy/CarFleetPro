namespace CarFleetPro.API.Models
{
    // Madde 6 — Hasar Kayıtları
    public class DamageRecord
    {
        public int DamageId { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public string DamageType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal EstimatedCost { get; set; }
        public DamageStatus Status { get; set; } = DamageStatus.IslemBekliyor;
        public string? PhotoUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
