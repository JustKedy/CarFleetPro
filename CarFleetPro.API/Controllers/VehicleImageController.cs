using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using CarFleetPro.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleImageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IStorageService _storage;

        public VehicleImageController(AppDbContext context, IStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/vehicleimage/{vehicleId}
        // Araçtaki tüm fotoğrafları listele (herkes erişebilir)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("{vehicleId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByVehicleId(int vehicleId)
        {
            var images = await _context.VehicleImages
                .Where(vi => vi.VehicleId == vehicleId)
                .OrderBy(vi => vi.DisplayOrder)
                .ThenByDescending(vi => vi.IsPrimary)
                .Select(vi => new VehicleImageDto
                {
                    VehicleImageId = vi.VehicleImageId,
                    VehicleId = vi.VehicleId,
                    ImageUrl = vi.ImageUrl,
                    PublicId = vi.PublicId,
                    IsPrimary = vi.IsPrimary,
                    DisplayOrder = vi.DisplayOrder,
                    UploadedAt = vi.UploadedAt
                })
                .ToListAsync();

            return Ok(images);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/vehicleimage/upload/{vehicleId}
        // Araç için fotoğraf yükle (Yönetici)
        // Form-data: file (IFormFile)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost("upload/{vehicleId:int}")]
        [Authorize(Roles = "Yönetici")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
        public async Task<IActionResult> Upload(int vehicleId, IFormFile file)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            // Araçtaki mevcut fotoğraf sayısını kontrol et (max 10)
            var existingCount = await _context.VehicleImages.CountAsync(vi => vi.VehicleId == vehicleId);
            if (existingCount >= 10)
                return BadRequest("Araç başına maksimum 10 fotoğraf yüklenebilir.");

            string imageUrl, publicId;
            try
            {
                (imageUrl, publicId) = await _storage.UploadAsync(file, $"vehicles/{vehicleId}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Yükleme sırasında hata oluştu: {ex.Message}");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // İlk fotoğraf otomatik olarak birincil yapılır
            var isPrimary = existingCount == 0;

            var image = new VehicleImage
            {
                VehicleId = vehicleId,
                ImageUrl = imageUrl,
                PublicId = publicId,
                IsPrimary = isPrimary,
                DisplayOrder = existingCount, // Sona ekle
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow
            };

            _context.VehicleImages.Add(image);

            // Birincil fotoğraf ise Vehicle.ImageUrl'i de güncelle
            if (isPrimary)
            {
                _context.Attach(vehicle);
                vehicle.ImageUrl = imageUrl;
                _context.Entry(vehicle).Property(v => v.ImageUrl).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new VehicleImageDto
            {
                VehicleImageId = image.VehicleImageId,
                VehicleId = image.VehicleId,
                ImageUrl = image.ImageUrl,
                PublicId = image.PublicId,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                UploadedAt = image.UploadedAt
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/vehicleimage/upload-multiple/{vehicleId}
        // Aynı anda birden fazla fotoğraf yükle (Yönetici)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost("upload-multiple/{vehicleId:int}")]
        [Authorize(Roles = "Yönetici")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB toplam
        public async Task<IActionResult> UploadMultiple(int vehicleId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("En az bir dosya seçmelisiniz.");

            if (files.Count > 10)
                return BadRequest("Tek seferde en fazla 10 fotoğraf yükleyebilirsiniz.");

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            var existingCount = await _context.VehicleImages.CountAsync(vi => vi.VehicleId == vehicleId);
            if (existingCount + files.Count > 10)
                return BadRequest($"Araç başına maksimum 10 fotoğraf yüklenebilir. Mevcut: {existingCount}, Eklenecek: {files.Count}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploaded = new List<VehicleImageDto>();
            var errors = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                try
                {
                    var (imageUrl, publicId) = await _storage.UploadAsync(file, $"vehicles/{vehicleId}");

                    var isPrimary = existingCount == 0 && i == 0; // Sadece ilk fotoğraf birincil olur

                    var image = new VehicleImage
                    {
                        VehicleId = vehicleId,
                        ImageUrl = imageUrl,
                        PublicId = publicId,
                        IsPrimary = isPrimary,
                        DisplayOrder = existingCount + i,
                        UploadedByUserId = userId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.VehicleImages.Add(image);

                    if (isPrimary)
                    {
                        _context.Attach(vehicle);
                        vehicle.ImageUrl = imageUrl;
                        _context.Entry(vehicle).Property(v => v.ImageUrl).IsModified = true;
                    }

                    await _context.SaveChangesAsync();

                    uploaded.Add(new VehicleImageDto
                    {
                        VehicleImageId = image.VehicleImageId,
                        VehicleId = image.VehicleId,
                        ImageUrl = image.ImageUrl,
                        PublicId = image.PublicId,
                        IsPrimary = image.IsPrimary,
                        DisplayOrder = image.DisplayOrder,
                        UploadedAt = image.UploadedAt
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"{file.FileName}: {ex.Message}");
                }
            }

            return Ok(new
            {
                uploaded,
                errors,
                message = $"{uploaded.Count}/{files.Count} fotoğraf yüklendi."
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/vehicleimage/{imageId}/set-primary
        // Birincil (kapak) fotoğrafı değiştir (Yönetici)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPut("{imageId:int}/set-primary")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> SetPrimary(int imageId)
        {
            var image = await _context.VehicleImages.FindAsync(imageId);
            if (image == null) return NotFound("Fotoğraf bulunamadı.");

            // Aynı araçtaki tüm fotoğraflardan birincil işaretini kaldır
            var allImages = await _context.VehicleImages
                .Where(vi => vi.VehicleId == image.VehicleId)
                .ToListAsync();

            foreach (var img in allImages)
            {
                _context.Attach(img);
                img.IsPrimary = img.VehicleImageId == imageId;
            }

            // Vehicle.ImageUrl'i de güncelle
            var vehicle = await _context.Vehicles.FindAsync(image.VehicleId);
            if (vehicle != null)
            {
                _context.Attach(vehicle);
                vehicle.ImageUrl = image.ImageUrl;
                _context.Entry(vehicle).Property(v => v.ImageUrl).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Birincil fotoğraf güncellendi." });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/vehicleimage/{vehicleId}/reorder
        // Fotoğraf sıralamasını güncelle (Yönetici)
        // Body: { "orders": { "imageId1": 0, "imageId2": 1, ... } }
        // ─────────────────────────────────────────────────────────────────────
        [HttpPut("{vehicleId:int}/reorder")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Reorder(int vehicleId, [FromBody] ReorderImagesDto dto)
        {
            var images = await _context.VehicleImages
                .Where(vi => vi.VehicleId == vehicleId)
                .ToListAsync();

            foreach (var image in images)
            {
                if (dto.Orders.TryGetValue(image.VehicleImageId, out var newOrder))
                {
                    _context.Attach(image);
                    image.DisplayOrder = newOrder;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Sıralama güncellendi." });
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/vehicleimage/{imageId}
        // Fotoğrafı Cloudinary'den ve DB'den sil (Yönetici)
        // ─────────────────────────────────────────────────────────────────────
        [HttpDelete("{imageId:int}")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Delete(int imageId)
        {
            var image = await _context.VehicleImages.FindAsync(imageId);
            if (image == null) return NotFound("Fotoğraf bulunamadı.");

            var vehicleId = image.VehicleId;
            var wasPrimary = image.IsPrimary;

            // Cloudinary'den sil
            await _storage.DeleteAsync(image.PublicId);

            // DB'den sil
            _context.VehicleImages.Remove(image);
            await _context.SaveChangesAsync();

            // Silinen birincil fotoğrafsa, sıradaki fotoğrafı birincil yap
            if (wasPrimary)
            {
                var nextImage = await _context.VehicleImages
                    .Where(vi => vi.VehicleId == vehicleId)
                    .OrderBy(vi => vi.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (nextImage != null)
                {
                    _context.Attach(nextImage);
                    nextImage.IsPrimary = true;

                    var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                    if (vehicle != null)
                    {
                        _context.Attach(vehicle);
                        vehicle.ImageUrl = nextImage.ImageUrl;
                        _context.Entry(vehicle).Property(v => v.ImageUrl).IsModified = true;
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Son fotoğraf da silindiyse Vehicle.ImageUrl'i temizle
                    var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                    if (vehicle != null)
                    {
                        _context.Attach(vehicle);
                        vehicle.ImageUrl = null;
                        _context.Entry(vehicle).Property(v => v.ImageUrl).IsModified = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok(new { message = "Fotoğraf silindi." });
        }
    }
}
