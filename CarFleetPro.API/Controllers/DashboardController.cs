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
        public async Task<IActionResult> GetDashboardStats([FromQuery] string? branch)
        {
            var vehiclesQuery = _context.Vehicles.AsQueryable();
            var rentalsQuery = _context.Rentals.AsQueryable();

            // 1. ŞUBE FİLTRESİ: Eğer Yunus yukarıdan "Bafra Şube" seçerse sadece orayı getir
            if (!string.IsNullOrWhiteSpace(branch) && branch.ToLower() != "tümü")
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.Branch == branch);
                
                // Seçilen şubedeki araçların ID'lerini bulup, sadece o araçların kiralamalarını alıyoruz
                var branchVehicleIds = vehiclesQuery.Select(v => v.VehicleId).ToList();
                rentalsQuery = rentalsQuery.Where(r => branchVehicleIds.Contains(r.VehicleId));
            }

            // 2. KART VERİLERİ (Toplam, Kirada, Müsait)
            var totalVehicles = await vehiclesQuery.CountAsync();
            var rentedVehicles = await vehiclesQuery.CountAsync(v => v.Status == VehicleStatus.Rented);
            var availableVehicles = await vehiclesQuery.CountAsync(v => v.Status == VehicleStatus.Available);
            var maintenanceVehicles = await vehiclesQuery.CountAsync(v => v.Status == VehicleStatus.Maintenance);

            // 3. AYLIK CİRO HESAPLAMA (Tüm zamanlar değil, sadece içinde bulunduğumuz ay)
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            
            var monthlyRevenue = await rentalsQuery
                .Where(r => r.Status == RentalStatus.Completed && 
                            r.StartDate.Month == currentMonth && 
                            r.StartDate.Year == currentYear)
                .SumAsync(r => r.TotalAmount);

            // 4. FİLO DURUM ANALİZİ (Yüzdelik Barlar İçin Matematik Şov)
            double rentedPct = totalVehicles > 0 ? Math.Round((double)rentedVehicles / totalVehicles * 100, 1) : 0;
            double availablePct = totalVehicles > 0 ? Math.Round((double)availableVehicles / totalVehicles * 100, 1) : 0;
            double maintenancePct = totalVehicles > 0 ? Math.Round((double)maintenanceVehicles / totalVehicles * 100, 1) : 0;

            // 5. EN ÇOK TALEP GÖREN MODELLER (Kiralanma sayısına göre ilk 2 arabayı çek)
            var topModels = await rentalsQuery
                .Join(_context.Vehicles, r => r.VehicleId, v => v.VehicleId, (r, v) => new { v.Brand, v.Model })
                .GroupBy(v => v.Brand + " " + v.Model)
                .Select(g => new TopModelDto { ModelName = g.Key, RentCount = g.Count() })
                .OrderByDescending(x => x.RentCount)
                .Take(2) // Tasarımda 2 tane görünüyor, o yüzden 2 tane yolluyoruz
                .ToListAsync();

            // Paketi topla ve Yunus'a yolla
            var stats = new DashboardStatsDto
            {
                TotalVehicles = totalVehicles,
                AvailableVehicles = availableVehicles,
                RentedVehicles = rentedVehicles,
                MonthlyRevenue = monthlyRevenue,
                RentedPercentage = rentedPct,
                AvailablePercentage = availablePct,
                MaintenancePercentage = maintenancePct,
                TopModels = topModels
            };

            return Ok(stats);
        }
    }
}