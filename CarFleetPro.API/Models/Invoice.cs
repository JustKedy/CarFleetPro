namespace CarFleetPro.API.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public int RentalId { get; set; }
        public Rental? Rental { get; set; }

        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public string? Notes { get; set; }
    }
}
