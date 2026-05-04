namespace CarFleetPro.API.DTOs
{
    // Madde 4 — Fatura DTO'ları
    public class CreateInvoiceDto
    {
        public int RentalId { get; set; }
        public DateTime TahsilatTarihi { get; set; }
        public decimal Tutar { get; set; }
        public Models.PaymentMethod OdemeYontemi { get; set; } = Models.PaymentMethod.Nakit;
        public string? Notes { get; set; }
    }

    public class InvoiceListDto
    {
        public int InvoiceId { get; set; }
        public int RentalId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public DateTime TahsilatTarihi { get; set; }
        public decimal Tutar { get; set; }
        public string OdemeYontemi { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
