using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RentalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RentalController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalDto dto)
        {
            // 1. Araç gerçekten var mı ve müsait mi? (Güvenli veri çekimi)
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);
            if (vehicle == null) return NotFound("Belirtilen araç bulunamadı.");
            if (vehicle.Status != VehicleStatus.Available)
                return BadRequest("Bu araç şu anda kiralanamaz. (Müsait değil)");

            // 2. Müşteri sistemde var mı? (Güvenli veri çekimi)
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
            if (customer == null) return NotFound("Belirtilen müşteri bulunamadı.");

            // 3. İşlemi yapan çalışanı (Seni) Token'ın içinden çekiyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            // 4. Otomatik Fiyat Hesaplama
            var days = (dto.PlannedEndDate - dto.StartDate).Days;
            if (days <= 0) days = 1; // En az 1 günlük kiralama yapılır
            var totalAmount = days * vehicle.DailyRate;

            // 5. Kiralama Fişini Oluştur
            var rental = new Rental
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                UserId = userId, // Token'dan gelen ID
                StartDate = dto.StartDate.ToUniversalTime(),
                PlannedEndDate = dto.PlannedEndDate.ToUniversalTime(),
                StartMileage = vehicle.Mileage,
                EndMileage = vehicle.Mileage,
                DailyRate = vehicle.DailyRate,
                TotalAmount = totalAmount,
                DepositAmount = dto.DepositAmount,
                Status = RentalStatus.Active, // Kiralama aktif
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            // İŞTE SİHİRLİ DOKUNUŞ BURADA BAŞLIYOR (Böl ve Yönet)

            // 1. HAMLE: Önce sadece Kiralama fişini veritabanına yazıp işlemi bitiriyoruz
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // 2. HAMLE: Sonra arabanın durumunu güncelleyip onu da AYRI olarak gönderiyoruz
            vehicle.Status = VehicleStatus.Rented;
            await _context.SaveChangesAsync();

            // Ve Mutlu Son...
            return Ok(new { 
                message = "Kiralama işlemi başarıyla tamamlandı!", 
                rentalId = rental.RentalId, 
                totalDays = days,
                totalAmount = totalAmount 
            });
        }
    }
}