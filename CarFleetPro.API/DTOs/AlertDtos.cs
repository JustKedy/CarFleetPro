namespace CarFleetPro.API.DTOs
{
    public class AlertDto
    {
        public string AlertType { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty; 
        public string Subtitle { get; set; } = string.Empty; 
        public string Detail { get; set; } = string.Empty; 
        public string AlertColor { get; set; } = "#EF4444"; 
        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
