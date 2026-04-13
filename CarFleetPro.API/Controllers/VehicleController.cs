using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache; // Cache servisini tanımladık
        private const string VehicleCacheKey = "vehicleList"; // Hafızadaki adımız
        private const string CardsCacheKey = "vehicleCards"; // Kartlar için cache key
        private const string CardsETagKey = "vehicleCardsETag"; // ETag cache key

        public VehicleController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllVehicles([FromQuery] string? status)
        {
            // out parametresini Nullable (?) yapıp null kontrolü ekledik
            if (!_cache.TryGetValue(VehicleCacheKey, out List<Vehicle>? vehicles) || vehicles == null)
            {
                vehicles = await _context.Vehicles.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _cache.Set(VehicleCacheKey, vehicles, cacheOptions);
            }

            // Ünlem işareti (!) ile derleyiciye "Bunun null olmadığını garanti ediyorum" dedik. Hata uçtu!
            var query = vehicles!.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "müsait")
                    query = query.Where(v => v.Status == VehicleStatus.Available);
                else if (status.ToLower() == "dolu")
                    query = query.Where(v => v.Status == VehicleStatus.Rented);
                else if (status.ToLower() == "bakımda")
                    query = query.Where(v => v.Status == VehicleStatus.Maintenance);
            }

            return Ok(query.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleDto dto)
        {
            if (await _context.Vehicles.AnyAsync(v => v.PlateNumber == dto.PlateNumber))
                return BadRequest("Bu plakaya sahip bir araç zaten filoda kayıtlı!");

            var newVehicle = new Vehicle
            {
                PlateNumber = dto.PlateNumber,
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                VehicleType = dto.VehicleType,
                FuelType = dto.FuelType,
                TransmissionType = dto.TransmissionType,
                DailyRate = dto.DailyRate,
                Mileage = dto.Mileage,
                HorsePower = dto.HorsePower,
                ImageUrl = dto.ImageUrl,
                Color = dto.Color,
                Branch = dto.Branch ?? "Merkez Şube", // Şube bilgisi ekleniyor
                Status = dto.Status, // Kullanıcının seçtiği durumu kaydet
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();
            InvalidateAllCaches(); // 🚀 Tüm cache'leri temizle

            return Ok(new { message = "Araç filoya başarıyla eklendi!", vehicle = newVehicle });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateVehicleDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            // 🚀 Global NoTracking aktif olduğu için Update'te entity'yi attach etmemiz gerekiyor
            _context.Attach(vehicle);

            vehicle.PlateNumber = dto.PlateNumber;
            vehicle.Brand = dto.Brand;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.VehicleType = dto.VehicleType;
            vehicle.FuelType = dto.FuelType;
            vehicle.TransmissionType = dto.TransmissionType;
            vehicle.DailyRate = dto.DailyRate;
            vehicle.Mileage = dto.Mileage;
            vehicle.HorsePower = dto.HorsePower;
            vehicle.ImageUrl = dto.ImageUrl;
            vehicle.Color = dto.Color;
            vehicle.Branch = dto.Branch ?? "Merkez Şube"; // Şube bilgisi güncelleniyor
            vehicle.Status = dto.Status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Entry(vehicle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            InvalidateAllCaches(); // 🚀 Tüm cache'leri temizle
            
            return Ok(new { message = "Araç başarıyla güncellendi!", vehicle });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Silinecek araç bulunamadı.");

            // Araba şu an kiradaysa silinmesine izin verme!
            if (vehicle.Status == VehicleStatus.Rented)
                return BadRequest("Bu araç şu anda kirada olduğu için sistemden silinemez!");

            // 🚀 Global NoTracking aktif → silmeden önce attach et
            _context.Attach(vehicle);
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            InvalidateAllCaches(); // 🚀 Tüm cache'leri temizle

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı araç filodan silindi." });
        }

        [HttpPost("upload-image")]
        public IActionResult UploadVehicleImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen bir görsel seçin.");

            // TODO: İleride resmi buluta (Supabase Storage vb.) yükleme kodlarını buraya yazacağız.
            // Şimdilik Yunus hata almasın diye ona başarılı bir sahte link dönüyoruz.
            var fakeImageUrl = "https://images.pexels.com/photos/170811/pexels-photo-170811.jpeg";

            return Ok(new { message = "Görsel başarıyla yüklendi", imageUrl = fakeImageUrl });
        }

        [HttpGet("last-updated")]
        [AllowAnonymous]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)] // 30sn cache (eskisi 60sn idi)
        public async Task<IActionResult> GetLastUpdated()
        {
            // 🚀 OPTİMİZASYON: GREATEST ile en büyük tarihi tek seferde bul
            // Eski kod: COALESCE mantığı yanlıştı — ilk non-null'u buluyordu, en büyüğü değil
            var lastUpdated = await _context.Database
                .SqlQueryRaw<DateTime?>(
                    @"SELECT GREATEST(
                        (SELECT MAX(GREATEST(""CreatedAt"", COALESCE(""UpdatedAt"", ""CreatedAt""))) FROM ""Vehicles""),
                        (SELECT MAX(""CreatedAt"") FROM ""Rentals"")
                    ) AS ""Value"""
                )
                .FirstOrDefaultAsync();

            return Ok(lastUpdated ?? DateTime.MinValue);
        }

        [HttpGet("cards")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleCardsForFrontend()
        {
            // 🚀 ETag DESTEĞİ: İstemci daha önce aldığı veriyi cache'leyip
            // If-None-Match header'ı ile gönderiyor. Veri değişmediyse 304 dönüyor → Sıfır transfer!
            var requestETag = Request.Headers.IfNoneMatch.FirstOrDefault();

            // Cache'den kontrol et — veri değişmediyse 304 dön
            if (!string.IsNullOrEmpty(requestETag) && 
                _cache.TryGetValue(CardsETagKey, out string? cachedETag) && 
                requestETag == cachedETag)
            {
                return StatusCode(304); // Not Modified — body boş, bant genişliği sıfır!
            }

            var currentYear = DateTime.UtcNow.Year;

            // 🚀 PgBouncer-SAFE: 2 basit sorgu (GroupJoin+SelectMany PgBouncer'da disposed connection hatası veriyordu)
            // Sorgu 1: Araç kartlarını çek (basit SELECT + projection)
            var cardList = await _context.Vehicles
                .Select(v => new VehicleCardDto
                {
                    Id = v.VehicleId,
                    Plaka = v.PlateNumber,
                    Marka = v.Brand,
                    Model = v.Model,
                    Hp = v.HorsePower,
                    Yas = currentYear - v.Year,
                    Km = v.Mileage,
                    Durum = v.Status == VehicleStatus.Available ? "MÜSAİT" :
                            v.Status == VehicleStatus.Rented ? "DOLU" : "BAKIMDA",
                    ResimUrl = v.ImageUrl ?? "https://via.placeholder.com/300"
                })
                .ToListAsync();

            // Sorgu 2: Aktif kiralamaları müşteri bilgisiyle çek (basit JOIN)
            var activeRentals = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Active)
                .Join(_context.Customers,
                    r => r.CustomerId,
                    c => c.CustomerId,
                    (r, c) => new
                    {
                        r.VehicleId,
                        r.TotalAmount,
                        r.StartDate,
                        r.PlannedEndDate,
                        KiralayanKisi = c.FirstName + " " + c.LastName
                    })
                .ToListAsync();

            // C# tarafında hızlı eşleme (Dictionary lookup = O(1))
            var rentalMap = activeRentals.ToDictionary(r => r.VehicleId);
            foreach (var card in cardList)
            {
                if (rentalMap.TryGetValue(card.Id, out var rental))
                {
                    card.KiralayanKisi = rental.KiralayanKisi;
                    card.KiralamaFiyati = rental.TotalAmount;
                    card.KiralamaSuresi = $"{(rental.PlannedEndDate - rental.StartDate).Days} Gün";
                    card.KiralamaTarihi = rental.StartDate.ToString("dd.MM.yyyy");
                }
            }



            // 🚀 ETag oluştur: Veri hash'lenerek unique bir parmak izi üretiliyor
            var json = JsonSerializer.Serialize(cardList);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
            var etag = $"\"{hash[..16]}\""; // İlk 16 karakter yeterli

            // Cache'e kaydet
            _cache.Set(CardsETagKey, etag, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));

            // Response'a ETag header'ı ekle
            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "private, max-age=60"; // Client 60sn cache'lesin

            return Ok(cardList);
        }

        [HttpPut("{id}/maintenance/start")]
        public async Task<IActionResult> SendToMaintenance(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");
            
            // Sadece müsait olan araçlar bakıma gidebilir (Kiradaki araba gidemez)
            if (vehicle.Status != VehicleStatus.Available) 
                return BadRequest("Sadece müsait durumdaki araçlar bakıma alınabilir.");

            // 🚀 Global NoTracking aktif → güncelleme için attach et
            _context.Attach(vehicle);
            vehicle.Status = VehicleStatus.Maintenance;
            _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
            await _context.SaveChangesAsync();
            InvalidateAllCaches();

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı araç bakıma alındı." });
        }

        [HttpPut("{id}/maintenance/end")]
        public async Task<IActionResult> EndMaintenance(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");
            
            if (vehicle.Status != VehicleStatus.Maintenance) 
                return BadRequest("Bu araç zaten bakımda değil.");

            // 🚀 Global NoTracking aktif → güncelleme için attach et
            _context.Attach(vehicle);
            vehicle.Status = VehicleStatus.Available;
            _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
            await _context.SaveChangesAsync();
            InvalidateAllCaches();

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı aracın bakımı bitti, tekrar müsait." });
        }

        /// <summary>
        /// GET /api/Vehicle/{id}/details — Araç detayı + kiralama ve bakım geçmişi
        /// </summary>
        [HttpGet("{id}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            // Kiralama geçmişi
            var rentals = await _context.Rentals
                .Where(r => r.VehicleId == id)
                .Join(_context.Customers, r => r.CustomerId, c => c.CustomerId,
                    (r, c) => new VehicleHistoryItemDto
                    {
                        Type = "Kiralama",
                        Title = $"Kiralandı: {c.FirstName} {c.LastName}",
                        DateRange = $"{r.StartDate:dd.MM.yyyy} - {r.PlannedEndDate:dd.MM.yyyy}",
                        Status = r.Status == RentalStatus.Active ? "Aktif" :
                                 r.Status == RentalStatus.Completed ? "Tamamlandı" : "İptal",
                        Amount = $"{r.TotalAmount:N0} TL",
                        Color = "#3B82F6"
                    })
                .ToListAsync();

            // Bakım geçmişi
            var maintenances = await _context.Maintenances
                .Where(m => m.VehicleId == id)
                .Select(m => new VehicleHistoryItemDto
                {
                    Type = "Bakım",
                    Title = m.Description,
                    DateRange = $"{m.StartDate:dd.MM.yyyy}",
                    Status = m.Status == MaintenanceStatus.Done ? "Tamamlandı" :
                             m.Status == MaintenanceStatus.InProgress ? "Devam Ediyor" : "Planlandı",
                    Amount = $"{m.Cost:N0} TL",
                    Color = "#F59E0B"
                })
                .ToListAsync();

            // Timeline birleştir ve sırala
            var history = rentals.Concat(maintenances)
                .OrderByDescending(h => h.DateRange)
                .Take(10) // Son 10 hareket
                .ToList();

            var detail = new VehicleDetailDto
            {
                VehicleId = vehicle.VehicleId,
                PlateNumber = vehicle.PlateNumber,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                VehicleType = vehicle.VehicleType.ToString(),
                FuelType = vehicle.FuelType.ToString(),
                TransmissionType = vehicle.TransmissionType.ToString(),
                DailyRate = vehicle.DailyRate,
                Status = vehicle.Status == VehicleStatus.Available ? "MÜSAİT" :
                         vehicle.Status == VehicleStatus.Rented ? "DOLU" : "BAKIMDA",
                Mileage = vehicle.Mileage,
                HorsePower = vehicle.HorsePower,
                Color = vehicle.Color,
                ImageUrl = vehicle.ImageUrl,
                Branch = vehicle.Branch,
                History = history
            };

            return Ok(detail);
        }

        /// <summary>
        /// 🚀 Tüm araç cache'lerini tek seferde temizle
        /// Veritabanı değiştiğinde hem liste cache'i hem kart cache'i hem ETag sıfırlanacak
        /// </summary>
        private void InvalidateAllCaches()
        {
            _cache.Remove(VehicleCacheKey);
            _cache.Remove(CardsCacheKey);
            _cache.Remove(CardsETagKey);
        }
    }
}