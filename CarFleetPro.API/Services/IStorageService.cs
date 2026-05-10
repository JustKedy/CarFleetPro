namespace CarFleetPro.API.Services
{
    public interface IStorageService
    {
        /// <summary>Dosyayı Cloudinary'ye yükler, (imageUrl, publicId) döner.</summary>
        Task<(string ImageUrl, string PublicId)> UploadAsync(IFormFile file, string folder = "vehicles");

        /// <summary>Cloudinary'den dosyayı siler.</summary>
        Task<bool> DeleteAsync(string publicId);
    }
}
