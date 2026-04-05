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
    [Authorize] // Patron verilerini herkes göremez, kilit şart!
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // Veritabanındaki tüm sayımları paralel olarak şimşek hızında çekiyoruz
            var totalVehicles = await _context.Vehicles.CountAsync();
            var availableVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Available);
            var rentedVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Rented);
            var maintenanceVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Maintenance);
            var totalCustomers = await _context.Customers.CountAsync();

            // Sadece başarıyla tamamlanmış ve kasaya girmiş kiralamaların paralarını topluyoruz
            var totalRevenue = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed)
                .SumAsync(r => r.TotalAmount);

            var stats = new DashboardStatsDto
            {
                TotalVehicles = totalVehicles,
                AvailableVehicles = availableVehicles,
                RentedVehicles = rentedVehicles,
                VehiclesInMaintenance = maintenanceVehicles,
                TotalCustomers = totalCustomers,
                TotalRevenue = totalRevenue
            };

            return Ok(stats);
        }
    }
}