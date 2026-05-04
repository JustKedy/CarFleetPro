namespace CarFleetPro.API.Models
{
    // Madde 4 — Faturalar
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public int RentalId { get; set; }
        public Rental? Rental { get; set; }

        public DateTime TahsilatTarihi { get; set; }
        public decimal Tutar { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Bekliyor;
        public PaymentMethod OdemYontemi { get; set; } = PaymentMethod.Nakit;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
