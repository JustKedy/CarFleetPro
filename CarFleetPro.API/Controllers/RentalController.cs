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

        // GET /api/rental — Tüm kiralamaları listele (filtre: status, customerId, vehicleId)
        [HttpGet]
        public async Task<IActionResult> GetAllRentals(
            [FromQuery] string? status,
            [FromQuery] int? customerId,
            [FromQuery] int? vehicleId)
        {
            var query = _context.Rentals
                .Join(_context.Customers, r => r.CustomerId, c => c.CustomerId,
                    (r, c) => new { r, c })
                .Join(_context.Vehicles, rc => rc.r.VehicleId, v => v.VehicleId,
                    (rc, v) => new { rc.r, rc.c, v })
                .AsQueryable();

            // Filtreler
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<RentalStatus>(status, true, out var parsedStatus))
                    query = query.Where(x => x.r.Status == parsedStatus);
                else if (status.ToLower() == "aktif")
                    query = query.Where(x => x.r.Status == RentalStatus.Active);
                else if (status.ToLower() == "tamamlandı" || status.ToLower() == "tamamlandi")
                    query = query.Where(x => x.r.Status == RentalStatus.Completed);
                else if (status.ToLower() == "iptal")
                    query = query.Where(x => x.r.Status == RentalStatus.Cancelled);
            }

            if (customerId.HasValue)
                query = query.Where(x => x.r.CustomerId == customerId.Value);

            if (vehicleId.HasValue)
                query = query.Where(x => x.r.VehicleId == vehicleId.Value);

            var result = await query
                .OrderByDescending(x => x.r.CreatedAt)
                .Select(x => new RentalListDto
                {
                    RentalId = x.r.RentalId,
                    CustomerName = x.c.FirstName + " " + x.c.LastName,
                    VehicleName = x.v.Brand + " " + x.v.Model,
                    PlateNumber = x.v.PlateNumber,
                    StartDate = x.r.StartDate,
                    PlannedEndDate = x.r.PlannedEndDate,
                    ActualEndDate = x.r.ActualEndDate,
                    DailyRate = x.r.DailyRate,
                    TotalAmount = x.r.TotalAmount,
                    DepositAmount = x.r.DepositAmount,
                    Status = x.r.Status == RentalStatus.Active ? "Aktif" :
                             x.r.Status == RentalStatus.Completed ? "Tamamlandı" : "İptal",
                    Notes = x.r.Notes,
                    CreatedAt = x.r.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /api/rental/{id} — Tek kiralama detayı
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRentalById(int id)
        {
            var item = await _context.Rentals
                .Join(_context.Customers, r => r.CustomerId, c => c.CustomerId,
                    (r, c) => new { r, c })
                .Join(_context.Vehicles, rc => rc.r.VehicleId, v => v.VehicleId,
                    (rc, v) => new { rc.r, rc.c, v })
                .FirstOrDefaultAsync(x => x.r.RentalId == id);

            if (item == null) return NotFound("Kiralama kaydı bulunamadı.");

            var dto = new RentalDetailDto
            {
                RentalId = item.r.RentalId,
                CustomerId = item.r.CustomerId,
                VehicleId = item.r.VehicleId,
                UserId = item.r.UserId,
                CustomerName = item.c.FirstName + " " + item.c.LastName,
                VehicleName = item.v.Brand + " " + item.v.Model,
                PlateNumber = item.v.PlateNumber,
                StartDate = item.r.StartDate,
                PlannedEndDate = item.r.PlannedEndDate,
                ActualEndDate = item.r.ActualEndDate,
                StartMileage = item.r.StartMileage,
                EndMileage = item.r.EndMileage,
                DailyRate = item.r.DailyRate,
                TotalAmount = item.r.TotalAmount,
                DepositAmount = item.r.DepositAmount,
                Status = item.r.Status == RentalStatus.Active ? "Aktif" :
                         item.r.Status == RentalStatus.Completed ? "Tamamlandı" : "İptal",
                Notes = item.r.Notes,
                CreatedAt = item.r.CreatedAt
            };

            return Ok(dto);
        }

        // POST /api/rental — Yeni kiralama oluştur
        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);
            if (vehicle == null) return NotFound("Belirtilen araç bulunamadı.");
            if (vehicle.Status != VehicleStatus.Available)
                return BadRequest("Bu araç şu anda kiralanamaz. (Müsait değil)");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
            if (customer == null) return NotFound("Belirtilen müşteri bulunamadı.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var days = (dto.PlannedEndDate - dto.StartDate).Days;
            if (days <= 0) days = 1;
            var totalAmount = days * vehicle.DailyRate;

            var rental = new Rental
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                UserId = userId,
                StartDate = dto.StartDate.ToUniversalTime(),
                PlannedEndDate = dto.PlannedEndDate.ToUniversalTime(),
                StartMileage = vehicle.Mileage,
                EndMileage = vehicle.Mileage,
                DailyRate = vehicle.DailyRate,
                TotalAmount = totalAmount,
                DepositAmount = dto.DepositAmount,
                Status = RentalStatus.Active,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Araç durumunu güncelle
            _context.Attach(vehicle);
            vehicle.Status = VehicleStatus.Rented;
            _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Kiralama işlemi başarıyla tamamlandı!",
                rentalId = rental.RentalId,
                totalDays = days,
                totalAmount = totalAmount
            });
        }

        // PUT /api/rental/{id}/complete — Kirayı tamamla
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteRental(int id, [FromBody] CompleteRentalDto dto)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);
            if (rental == null) return NotFound("Kiralama kaydı bulunamadı.");
            if (rental.Status != RentalStatus.Active)
                return BadRequest("Sadece aktif kiralamalar tamamlanabilir.");

            _context.Attach(rental);
            rental.Status = RentalStatus.Completed;
            rental.ActualEndDate = (dto.ActualEndDate ?? DateTime.UtcNow).ToUniversalTime();
            rental.EndMileage = dto.EndMileage;
            if (!string.IsNullOrEmpty(dto.Notes))
                rental.Notes = dto.Notes;

            _context.Entry(rental).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Aracı tekrar müsait yap
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == rental.VehicleId);
            if (vehicle != null)
            {
                _context.Attach(vehicle);
                vehicle.Status = VehicleStatus.Available;
                vehicle.Mileage = dto.EndMileage;
                _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
                _context.Entry(vehicle).Property(v => v.Mileage).IsModified = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Kiralama tamamlandı, araç müsait duruma alındı." });
        }

        // PUT /api/rental/{id}/cancel — Kiralamasını iptal et
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRental(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);
            if (rental == null) return NotFound("Kiralama kaydı bulunamadı.");
            if (rental.Status == RentalStatus.Completed)
                return BadRequest("Tamamlanmış bir kiralama iptal edilemez.");

            _context.Attach(rental);
            rental.Status = RentalStatus.Cancelled;
            _context.Entry(rental).Property(r => r.Status).IsModified = true;
            await _context.SaveChangesAsync();

            // Araç aktifse müsait yap
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == rental.VehicleId);
            if (vehicle != null && vehicle.Status == VehicleStatus.Rented)
            {
                _context.Attach(vehicle);
                vehicle.Status = VehicleStatus.Available;
                _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Kiralama iptal edildi." });
        }

        // DELETE /api/rental/{id} — Kiralama kaydını sil (sadece iptal edilmiş olanlar)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRental(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);
            if (rental == null) return NotFound("Kiralama kaydı bulunamadı.");
            if (rental.Status == RentalStatus.Active)
                return BadRequest("Aktif bir kiralama silinemez. Önce iptal edin.");

            _context.Attach(rental);
            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kiralama kaydı silindi." });
        }
    }
}