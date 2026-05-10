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

        /// <summary>
        /// GET /api/rental — Tüm kiralamaları listele.
        /// Yönetici: hepsini görür. Çalışan: hepsini görür (para bilgisi hariç filtreleme yapılmaz).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRentals([FromQuery] string? status)
        {
            var query = _context.Rentals
                .Join(_context.Customers, r => r.CustomerId, c => c.CustomerId,
                    (r, c) => new { Rental = r, Customer = c })
                .Join(_context.Vehicles, rc => rc.Rental.VehicleId, v => v.VehicleId,
                    (rc, v) => new { rc.Rental, rc.Customer, Vehicle = v })
                .AsQueryable();

            // Durum filtresi
            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "active")
                    query = query.Where(x => x.Rental.Status == RentalStatus.Active);
                else if (status.ToLower() == "completed")
                    query = query.Where(x => x.Rental.Status == RentalStatus.Completed);
                else if (status.ToLower() == "cancelled")
                    query = query.Where(x => x.Rental.Status == RentalStatus.Cancelled);
            }

            var isAdmin = User.IsInRole("Yönetici");

            var result = await query
                .OrderByDescending(x => x.Rental.CreatedAt)
                .Select(x => new RentalListDto
                {
                    RentalId = x.Rental.RentalId,
                    CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                    VehiclePlate = x.Vehicle.PlateNumber,
                    VehicleName = x.Vehicle.Brand + " " + x.Vehicle.Model,
                    StartDate = x.Rental.StartDate,
                    PlannedEndDate = x.Rental.PlannedEndDate,
                    ActualEndDate = x.Rental.ActualEndDate,
                    DailyRate = x.Rental.DailyRate,
                    // Çalışan toplam tutarı görebilir ama bu veri dashboard'da gizlenir
                    TotalAmount = x.Rental.TotalAmount,
                    DepositAmount = x.Rental.DepositAmount,
                    Status = x.Rental.Status == RentalStatus.Active ? "Aktif" :
                             x.Rental.Status == RentalStatus.Completed ? "Tamamlandı" : "İptal",
                    Notes = x.Rental.Notes,
                    CreatedAt = x.Rental.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>GET /api/rental/{id} — Kiralama detayı</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRentalById(int id)
        {
            var item = await _context.Rentals
                .Join(_context.Customers, r => r.CustomerId, c => c.CustomerId,
                    (r, c) => new { Rental = r, Customer = c })
                .Join(_context.Vehicles, rc => rc.Rental.VehicleId, v => v.VehicleId,
                    (rc, v) => new { rc.Rental, rc.Customer, Vehicle = v })
                .FirstOrDefaultAsync(x => x.Rental.RentalId == id);

            if (item == null) return NotFound("Kiralama bulunamadı.");

            return Ok(new RentalListDto
            {
                RentalId = item.Rental.RentalId,
                CustomerName = item.Customer.FirstName + " " + item.Customer.LastName,
                VehiclePlate = item.Vehicle.PlateNumber,
                VehicleName = item.Vehicle.Brand + " " + item.Vehicle.Model,
                StartDate = item.Rental.StartDate,
                PlannedEndDate = item.Rental.PlannedEndDate,
                ActualEndDate = item.Rental.ActualEndDate,
                DailyRate = item.Rental.DailyRate,
                TotalAmount = item.Rental.TotalAmount,
                DepositAmount = item.Rental.DepositAmount,
                Status = item.Rental.Status == RentalStatus.Active ? "Aktif" :
                         item.Rental.Status == RentalStatus.Completed ? "Tamamlandı" : "İptal",
                Notes = item.Rental.Notes,
                CreatedAt = item.Rental.CreatedAt
            });
        }

        /// <summary>POST /api/rental — Yeni kiralama oluştur (Admin + Çalışan)</summary>
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

        /// <summary>
        /// PUT /api/rental/{id}/complete — Kiralama tamamla (araç teslim alındı)
        /// Admin + Çalışan erişebilir.
        /// </summary>
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteRental(int id, [FromBody] CompleteRentalDto dto)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound("Kiralama bulunamadı.");
            if (rental.Status != RentalStatus.Active)
                return BadRequest("Sadece aktif kiralamalar tamamlanabilir.");

            _context.Attach(rental);
            rental.Status = RentalStatus.Completed;
            rental.ActualEndDate = dto.ActualEndDate.ToUniversalTime();
            rental.EndMileage = dto.EndMileage;

            // Gerçek gün sayısına göre toplam tutarı yeniden hesapla
            var actualDays = (dto.ActualEndDate - rental.StartDate).Days;
            if (actualDays <= 0) actualDays = 1;
            rental.TotalAmount = actualDays * rental.DailyRate;

            _context.Entry(rental).Property(r => r.Status).IsModified = true;
            _context.Entry(rental).Property(r => r.ActualEndDate).IsModified = true;
            _context.Entry(rental).Property(r => r.EndMileage).IsModified = true;
            _context.Entry(rental).Property(r => r.TotalAmount).IsModified = true;
            await _context.SaveChangesAsync();

            // Araç durumunu müsait yap
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle != null)
            {
                _context.Attach(vehicle);
                vehicle.Status = VehicleStatus.Available;
                vehicle.Mileage = dto.EndMileage;
                _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
                _context.Entry(vehicle).Property(v => v.Mileage).IsModified = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Kiralama tamamlandı. Araç müsait duruma alındı.",
                totalAmount = rental.TotalAmount,
                actualDays
            });
        }

        /// <summary>
        /// PUT /api/rental/{id}/cancel — Kiralama iptal et.
        /// Sadece Yönetici iptal edebilir.
        /// </summary>
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> CancelRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound("Kiralama bulunamadı.");
            if (rental.Status != RentalStatus.Active)
                return BadRequest("Sadece aktif kiralamalar iptal edilebilir.");

            _context.Attach(rental);
            rental.Status = RentalStatus.Cancelled;
            _context.Entry(rental).Property(r => r.Status).IsModified = true;
            await _context.SaveChangesAsync();

            // Araç müsait yap
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle != null)
            {
                _context.Attach(vehicle);
                vehicle.Status = VehicleStatus.Available;
                _context.Entry(vehicle).Property(v => v.Status).IsModified = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Kiralama iptal edildi. Araç müsait duruma alındı." });
        }
    }
}