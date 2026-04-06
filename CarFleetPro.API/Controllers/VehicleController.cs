using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

        public VehicleController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> GetAllVehicles([FromQuery] string? status)
        {
            // 1. Önce listeyi Cache'den (Hafızadan) almayı deniyoruz
            if (!_cache.TryGetValue(VehicleCacheKey, out List<Vehicle> vehicles))
            {
                // 2. Eğer hafızada yoksa (ilk defa çalışıyorsa veya yeni araç eklendiyse) DB'den çek
                vehicles = await _context.Vehicles.ToListAsync();

                // 3. Çektiğin bu veriyi hafızaya kaydet (Ömrü: Biz silene kadar veya uygulama kapanana kadar)
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _cache.Set(VehicleCacheKey, vehicles, cacheOptions);
            }

            // 4. Hafızadaki liste üzerinden Yunus'un filtrelerini (Müsait, Dolu vs.) uygula
            var query = vehicles.AsQueryable();

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
            _cache.Remove(VehicleCacheKey); // Veritabanı değişti, eski hafızayı sil!

            return Ok(new { message = "Araç filoya başarıyla eklendi!", vehicle = newVehicle });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateVehicleDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

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

            await _context.SaveChangesAsync();
            _cache.Remove(VehicleCacheKey); // Veritabanı değişti, eski hafızayı sil!
            
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

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            _cache.Remove(VehicleCacheKey); // Veritabanı değişti, eski hafızayı sil!

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
        public async Task<IActionResult> GetLastUpdated()
        {
            var maxVehicleCreatedAt = await _context.Vehicles.MaxAsync(v => (DateTime?)v.CreatedAt);
            var maxVehicleUpdatedAt = await _context.Vehicles.MaxAsync(v => v.UpdatedAt); // Already nullable
            var maxRentalCreatedAt = await _context.Rentals.MaxAsync(r => (DateTime?)r.CreatedAt);

            var lastUpdated = new[] 
            { 
                maxVehicleCreatedAt ?? DateTime.MinValue, 
                maxVehicleUpdatedAt ?? DateTime.MinValue, 
                maxRentalCreatedAt ?? DateTime.MinValue 
            }.Max();

            return Ok(lastUpdated);
        }

        [HttpGet("cards")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleCardsForFrontend()
        {
            var currentYear = DateTime.UtcNow.Year;

            // Tüm arabaları, aktif kiralamaları ve müşterileri alıyoruz
            var vehicles = await _context.Vehicles.ToListAsync();
            var activeRentals = await _context.Rentals.Where(r => r.Status == RentalStatus.Active).ToListAsync();
            var customers = await _context.Customers.ToListAsync();

            var cardList = new List<VehicleCardDto>();

            foreach (var v in vehicles)
            {
                // Bu araç şu an kirada mı diye bakıyoruz
                var rental = activeRentals.FirstOrDefault(r => r.VehicleId == v.VehicleId);
                var customer = rental != null ? customers.FirstOrDefault(c => c.CustomerId == rental.CustomerId) : null;

                cardList.Add(new VehicleCardDto
                {
                    Id = v.VehicleId,
                    Plaka = v.PlateNumber,
                    Marka = v.Brand,
                    Model = v.Model,
                    Hp = v.HorsePower,
                    Yas = currentYear - v.Year, // Yaşı otomatik hesaplıyoruz!
                    Km = v.Mileage,
                    Durum = v.Status == VehicleStatus.Available ? "MÜSAİT" :
                            v.Status == VehicleStatus.Rented ? "DOLU" : "BAKIMDA",

                    // Eğer kiralıysa bilgileri doldur, değilse null bırak
                    KiralayanKisi = customer != null ? $"{customer.FirstName} {customer.LastName}" : null,
                    KiralamaFiyati = rental?.TotalAmount,
                    KiralamaSuresi = rental != null ? $"{(rental.PlannedEndDate - rental.StartDate).Days} Gün" : null,
                    KiralamaTarihi = rental?.StartDate.ToString("dd.MM.yyyy"),

                    ResimUrl = v.ImageUrl ?? "https://via.placeholder.com/300" // Resim yoksa boş gri bir resim koyar
                });
            }

            return Ok(cardList); // Kadir'in istediği o kusursuz JSON paketini yolla!
        }
    }
}