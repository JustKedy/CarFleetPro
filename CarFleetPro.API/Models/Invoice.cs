namespace CarFleetPro.API.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        // İlgili kiralama
        public int RentalId { get; set; }
        public Rental? Rental { get; set; }

        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        // Fatura açıklaması
        public string? Notes { get; set; }
    }
}
