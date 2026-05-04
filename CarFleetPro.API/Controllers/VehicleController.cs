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
        private readonly IMemoryCache _cache; 
        private const string VehicleCacheKey = "vehicleList"; 
        private const string CardsCacheKey = "vehicleCards"; 
        private const string CardsETagKey = "vehicleCardsETag"; 

        public VehicleController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllVehicles([FromQuery] string? status)
        {
            
            if (!_cache.TryGetValue(VehicleCacheKey, out List<Vehicle>? vehicles) || vehicles == null)
            {
                vehicles = await _context.Vehicles.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _cache.Set(VehicleCacheKey, vehicles, cacheOptions);
            }

            
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
                Branch = dto.Branch ?? "Merkez Şube", 
                Status = dto.Status, 
                InsuranceExpiry = dto.InsuranceExpiry,
                InspectionExpiry = dto.InspectionExpiry,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();
            InvalidateAllCaches(); 

            return Ok(new { message = "Araç filoya başarıyla eklendi!", vehicle = newVehicle });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateVehicleDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            
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
            vehicle.Branch = dto.Branch ?? "Merkez Şube"; 
            vehicle.Status = dto.Status;
            vehicle.InsuranceExpiry = dto.InsuranceExpiry;
            vehicle.InspectionExpiry = dto.InspectionExpiry;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Entry(vehicle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            InvalidateAllCaches(); 
            
            return Ok(new { message = "Araç başarıyla güncellendi!", vehicle });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Silinecek araç bulunamadı.");

            
            if (vehicle.Status == VehicleStatus.Rented)
                return BadRequest("Bu araç şu anda kirada olduğu için sistemden silinemez!");

            
            _context.Attach(vehicle);
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            InvalidateAllCaches(); 

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı araç filodan silindi." });
        }

        [HttpPost("upload-image")]
        public IActionResult UploadVehicleImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen bir görsel seçin.");

            
            
            var fakeImageUrl = "https://images.pexels.com/photos/170811/pexels-photo-170811.jpeg";

            return Ok(new { message = "Görsel başarıyla yüklendi", imageUrl = fakeImageUrl });
        }

        [HttpGet("last-updated")]
        [AllowAnonymous]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)] 
        public async Task<IActionResult> GetLastUpdated()
        {
            
            
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
            
            
            var requestETag = Request.Headers.IfNoneMatch.FirstOrDefault();

            
            if (!string.IsNullOrEmpty(requestETag) && 
                _cache.TryGetValue(CardsETagKey, out string? cachedETag) && 
                requestETag == cachedETag)
            {
                return StatusCode(304); 
            }

            var currentYear = DateTime.UtcNow.Year;

            
            
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
                    DailyRate = v.DailyRate,
                    Durum = v.Status == VehicleStatus.Available ? "MÜSAİT" :
                            v.Status == VehicleStatus.Rented ? "DOLU" : "BAKIMDA",
                    ResimUrl = v.ImageUrl ?? "https://via.placeholder.com/300"
                })
                .ToListAsync();

            
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

            
            var json = JsonSerializer.Serialize(cardList);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
            var etag = $"\"{hash[..16]}\""; 

            
            _cache.Set(CardsETagKey, etag, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));

            
            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "private, max-age=60"; 

            return Ok(cardList);
        }

        [HttpPut("{id}/maintenance/start")]
        public async Task<IActionResult> SendToMaintenance(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");
            
            
            if (vehicle.Status != VehicleStatus.Available) 
                return BadRequest("Sadece müsait durumdaki araçlar bakıma alınabilir.");

            
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

            
            _context.Attach(vehicle);
            vehicle.Status = VehicleStatus.Available;
            _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
            await _context.SaveChangesAsync();
            InvalidateAllCaches();

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı aracın bakımı bitti, tekrar müsait." });
        }

        
        
        
        [HttpGet("{id}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            
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

            
            var history = rentals.Concat(maintenances)
                .OrderByDescending(h => h.DateRange)
                .Take(10) 
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

        
        
        
        
        private void InvalidateAllCaches()
        {
            _cache.Remove(VehicleCacheKey);
            _cache.Remove(CardsCacheKey);
            _cache.Remove(CardsETagKey);
        }
    }
}