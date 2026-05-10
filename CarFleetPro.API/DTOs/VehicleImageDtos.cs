namespace CarFleetPro.API.DTOs
{
    public class VehicleImageDto
    {
        public int VehicleImageId { get; set; }
        public int VehicleId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class SetPrimaryImageDto
    {
        public int VehicleImageId { get; set; }
    }

    public class ReorderImagesDto
    {
        /// <summary>Key = VehicleImageId, Value = yeni DisplayOrder</summary>
        public Dictionary<int, int> Orders { get; set; } = new();
    }
}
