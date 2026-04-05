using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return Ok(vehicles);
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
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Araç filoya başarıyla eklendi!", vehicle = newVehicle });
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

        [HttpPut("{id}/maintenance/start")]
        public async Task<IActionResult> SendToMaintenance(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");
            
            // Sadece müsait olan araçlar bakıma gidebilir (Kiradaki araba gidemez)
            if (vehicle.Status != VehicleStatus.Available) 
                return BadRequest("Sadece müsait durumdaki araçlar bakıma alınabilir.");

            vehicle.Status = VehicleStatus.Maintenance;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı araç bakıma alındı." });
        }

        [HttpPut("{id}/maintenance/end")]
        public async Task<IActionResult> EndMaintenance(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadı.");
            
            if (vehicle.Status != VehicleStatus.Maintenance) 
                return BadRequest("Bu araç zaten bakımda değil.");

            vehicle.Status = VehicleStatus.Available;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{vehicle.PlateNumber} plakalı aracın bakımı bitti, tekrar müsait." });
        }
    }
}