namespace CarFleetPro.API.Models
{
    public class VehicleImage
    {
        public int VehicleImageId { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        // Cloudinary'den dönen kalıcı URL
        public string ImageUrl { get; set; } = string.Empty;

        // Cloudinary'den dönen PublicId — silme işlemi için gerekli
        public string PublicId { get; set; } = string.Empty;

        // Araçta gösterilecek ana (kapak) fotoğraf mı?
        public bool IsPrimary { get; set; } = false;

        // Sıralama (0 = ilk)
        public int DisplayOrder { get; set; } = 0;

        // Kim yükledi
        public string? UploadedByUserId { get; set; }
        public AppUser? UploadedByUser { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
