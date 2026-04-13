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

            
            return Ok(new { 
                message = "Kiralama işlemi başarıyla tamamlandı!", 
                rentalId = rental.RentalId, 
                totalDays = days,
                totalAmount = totalAmount 
            });
        }
    }
}