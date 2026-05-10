namespace CarFleetPro.API.DTOs
{
    public class CreateInvoiceDto
    {
        public int RentalId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateInvoiceDto
    {
        /// <summary>"Pending", "Paid", "Cancelled"</summary>
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
    }

    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int RentalId { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Kiralama özet bilgisi
        public string? CustomerName { get; set; }
        public string? VehiclePlate { get; set; }
    }
}
