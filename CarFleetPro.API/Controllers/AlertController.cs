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
    public class AlertController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/Alert — Tüm bildirimleri getir (gecikmiş iadeler + bakım uyarıları)
        /// Ayrı bir Alert tablosu yok — Rentals + Vehicles + Maintenances'dan türetiliyor
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var today = DateTime.UtcNow;
            var alerts = new List<AlertDto>();

            // 1. GECİKMİŞ İADELER — PlannedEndDate geçmiş ama hâlâ aktif olan kiralamalar
            var overdueRentals = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Active && r.PlannedEndDate < today)
                .Join(_context.Vehicles, r => r.VehicleId, v => v.VehicleId,
                    (r, v) => new { Rental = r, Vehicle = v })
                .Join(_context.Customers, rv => rv.Rental.CustomerId, c => c.CustomerId,
                    (rv, c) => new { rv.Rental, rv.Vehicle, Customer = c })
                .ToListAsync();

            foreach (var item in overdueRentals)
            {
                var delayDays = (today - item.Rental.PlannedEndDate).Days;
                alerts.Add(new AlertDto
                {
                    AlertType = "GECİKMİŞ İADE",
                    Title = $"{item.Vehicle.PlateNumber} - {item.Vehicle.Brand} {item.Vehicle.Model}",
                    Subtitle = $"Müşteri: {item.Customer.FirstName} {item.Customer.LastName}",
                    Detail = $"{delayDays} Gündür Gecikmede",
                    AlertColor = "#EF4444", // Kırmızı
                    RelatedVehicleId = item.Vehicle.VehicleId,
                    RelatedRentalId = item.Rental.RentalId,
                    CreatedAt = item.Rental.PlannedEndDate
                });
            }

            // 2. YAKIN TARİHLİ İADELER — 2 gün içinde iade tarihi dolacak aktif kiralamalar
            var upcomingReturns = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Active
                         && r.PlannedEndDate >= today
                         && r.PlannedEndDate <= today.AddDays(2))
                .Join(_context.Vehicles, r => r.VehicleId, v => v.VehicleId,
                    (r, v) => new { Rental = r, Vehicle = v })
                .Join(_context.Customers, rv => rv.Rental.CustomerId, c => c.CustomerId,
                    (rv, c) => new { rv.Rental, rv.Vehicle, Customer = c })
                .ToListAsync();

            foreach (var item in upcomingReturns)
            {
                var daysLeft = (item.Rental.PlannedEndDate - today).Days;
                alerts.Add(new AlertDto
                {
                    AlertType = "İADE YAKLASTI",
                    Title = $"{item.Vehicle.PlateNumber} - {item.Vehicle.Brand} {item.Vehicle.Model}",
                    Subtitle = $"Müşteri: {item.Customer.FirstName} {item.Customer.LastName}",
                    Detail = daysLeft == 0 ? "Bugün İade Edilmeli" : $"{daysLeft} Gün Kaldı",
                    AlertColor = "#F59E0B", // Sarı/Turuncu
                    RelatedVehicleId = item.Vehicle.VehicleId,
                    RelatedRentalId = item.Rental.RentalId,
                    CreatedAt = today
                });
            }

            // 3. BAKIM UYARILARI — Bakımda olan araçlar
            var maintenanceVehicles = await _context.Vehicles
                .Where(v => v.Status == VehicleStatus.Maintenance)
                .ToListAsync();

            foreach (var vehicle in maintenanceVehicles)
            {
                alerts.Add(new AlertDto
                {
                    AlertType = "BAKIMDA",
                    Title = $"{vehicle.PlateNumber} - {vehicle.Brand} {vehicle.Model}",
                    Subtitle = $"Şube: {vehicle.Branch}",
                    Detail = "Araç bakımda bekliyor",
                    AlertColor = "#F59E0B",
                    RelatedVehicleId = vehicle.VehicleId,
                    CreatedAt = vehicle.UpdatedAt ?? vehicle.CreatedAt
                });
            }

            // Önce gecikmiş iadeler, sonra yaklaşan iadeler, sonra bakımlar
            var sortedAlerts = alerts
                .OrderByDescending(a => a.AlertType == "GECİKMİŞ İADE")
                .ThenByDescending(a => a.AlertType == "İADE YAKLASTI")
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            return Ok(sortedAlerts);
        }
    }
}
