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

            
            if (!string.IsNullOrWhiteSpace(branch) && branch.ToLower() != "tümü")
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.Branch == branch);
                
                
                var branchVehicleIds = vehiclesQuery.Select(v => v.VehicleId).ToList();
                rentalsQuery = rentalsQuery.Where(r => branchVehicleIds.Contains(r.VehicleId));
            }

            
            
            
            var fleetStats = await vehiclesQuery
                .GroupBy(v => 1) 
                .Select(g => new
                {
                    Total = g.Count(),
                    Rented = g.Count(v => v.Status == VehicleStatus.Rented),
                    Available = g.Count(v => v.Status == VehicleStatus.Available),
                    Maintenance = g.Count(v => v.Status == VehicleStatus.Maintenance)
                })
                .FirstOrDefaultAsync();

            int totalVehicles = fleetStats?.Total ?? 0;
            int rentedVehicles = fleetStats?.Rented ?? 0;
            int availableVehicles = fleetStats?.Available ?? 0;
            int maintenanceVehicles = fleetStats?.Maintenance ?? 0;

            
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            
            var monthlyRevenue = await rentalsQuery
                .Where(r => r.Status == RentalStatus.Completed && 
                            r.StartDate.Month == currentMonth && 
                            r.StartDate.Year == currentYear)
                .SumAsync(r => r.TotalAmount);

            
            double rentedPct = totalVehicles > 0 ? Math.Round((double)rentedVehicles / totalVehicles * 100, 1) : 0;
            double availablePct = totalVehicles > 0 ? Math.Round((double)availableVehicles / totalVehicles * 100, 1) : 0;
            double maintenancePct = totalVehicles > 0 ? Math.Round((double)maintenanceVehicles / totalVehicles * 100, 1) : 0;

            
            var topModels = await rentalsQuery
                .Join(_context.Vehicles, r => r.VehicleId, v => v.VehicleId, (r, v) => new { v.Brand, v.Model })
                .GroupBy(v => v.Brand + " " + v.Model)
                .Select(g => new TopModelDto { ModelName = g.Key, RentCount = g.Count() })
                .OrderByDescending(x => x.RentCount)
                .Take(2) 
                .ToListAsync();

            
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