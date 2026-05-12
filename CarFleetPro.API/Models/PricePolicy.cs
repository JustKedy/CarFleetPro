namespace CarFleetPro.API.Models
{
    public class PricePolicy
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Global, Segment, Vehicle
        /// </summary>
        public string TargetType { get; set; } = "Global";
        
        /// <summary>
        /// Segment name (SUV, Eko etc.) or Vehicle Plate if needed
        /// </summary>
        public string? TargetValue { get; set; }
        
        public decimal BasePrice { get; set; }
        public double MaxDiscountPercentage { get; set; }
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
