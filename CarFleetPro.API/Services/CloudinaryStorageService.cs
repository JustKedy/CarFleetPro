using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace CarFleetPro.API.Services
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloudName = configuration["CloudinarySettings:CloudName"]!;
            var apiKey    = configuration["CloudinarySettings:ApiKey"]!;
            var apiSecret = configuration["CloudinarySettings:ApiSecret"]!;

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<(string ImageUrl, string PublicId)> UploadAsync(IFormFile file, string folder = "vehicles")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Geçersiz dosya.");

            // Sadece resim dosyalarına izin ver
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Sadece JPEG, PNG ve WebP formatları kabul edilmektedir.");

            // Maksimum 10 MB
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("Dosya boyutu 10 MB'ı geçemez.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"carfleetpro/{folder}",
                // Otomatik kalite optimizasyonu ve WebP dönüşümü
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto"),
                // Benzersiz PublicId
                PublicId = $"{folder}/{Guid.NewGuid():N}"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary yükleme hatası: {result.Error.Message}");

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
    }
}
