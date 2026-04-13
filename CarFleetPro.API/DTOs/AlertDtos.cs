namespace CarFleetPro.API.DTOs
{
    public class AlertDto
    {
        public string AlertType { get; set; } = string.Empty; // "GECİKMİŞ İADE", "BAKIM UYARISI", "MÜSAİT ARAÇ"
        public string Title { get; set; } = string.Empty; // "55 MS 061 - Renault Clio"
        public string Subtitle { get; set; } = string.Empty; // "Müşteri: Abdulkadir Toksöz"
        public string Detail { get; set; } = string.Empty; // "2 Gündür Gecikmede"
        public string AlertColor { get; set; } = "#EF4444"; // Renk kodu
        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
