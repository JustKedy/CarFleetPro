namespace CarFleetPro.API.Models
{
    public class VehicleImage
    {
        public int VehicleImageId { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string PublicId { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public string? UploadedByUserId { get; set; }
        public AppUser? UploadedByUser { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
