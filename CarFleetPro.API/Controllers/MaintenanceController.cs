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
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/maintenance — Tüm bakım kayıtlarını listele (Admin + Çalışan)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? vehicleId, [FromQuery] string? status)
        {
            var query = _context.Maintenances
                .Join(_context.Vehicles, m => m.VehicleId, v => v.VehicleId,
                    (m, v) => new { Maintenance = m, Vehicle = v })
                .AsQueryable();

            if (vehicleId.HasValue)
                query = query.Where(x => x.Vehicle.VehicleId == vehicleId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "planned")
                    query = query.Where(x => x.Maintenance.Status == MaintenanceStatus.Planned);
                else if (status.ToLower() == "inprogress")
                    query = query.Where(x => x.Maintenance.Status == MaintenanceStatus.InProgress);
                else if (status.ToLower() == "done")
                    query = query.Where(x => x.Maintenance.Status == MaintenanceStatus.Done);
            }

            var result = await query
                .OrderByDescending(x => x.Maintenance.CreatedAt)
                .Select(x => new MaintenanceDto
                {
                    MaintenanceId = x.Maintenance.MaintenanceId,
                    VehicleId = x.Vehicle.VehicleId,
                    VehiclePlate = x.Vehicle.PlateNumber,
                    VehicleName = x.Vehicle.Brand + " " + x.Vehicle.Model,
                    MaintenanceType = x.Maintenance.MaintenanceType.ToString(),
                    Description = x.Maintenance.Description,
                    StartDate = x.Maintenance.StartDate,
                    EndDate = x.Maintenance.EndDate,
                    NextInspectionDate = x.Maintenance.NextInspectionDate,
                    Cost = x.Maintenance.Cost,
                    Status = x.Maintenance.Status == MaintenanceStatus.Planned ? "Planlandı" :
                             x.Maintenance.Status == MaintenanceStatus.InProgress ? "Devam Ediyor" : "Tamamlandı",
                    CreatedAt = x.Maintenance.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>GET /api/maintenance/{id} — Bakım detayı</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Maintenances
                .Join(_context.Vehicles, m => m.VehicleId, v => v.VehicleId,
                    (m, v) => new { Maintenance = m, Vehicle = v })
                .FirstOrDefaultAsync(x => x.Maintenance.MaintenanceId == id);

            if (item == null) return NotFound("Bakım kaydı bulunamadı.");

            return Ok(new MaintenanceDto
            {
                MaintenanceId = item.Maintenance.MaintenanceId,
                VehicleId = item.Vehicle.VehicleId,
                VehiclePlate = item.Vehicle.PlateNumber,
                VehicleName = item.Vehicle.Brand + " " + item.Vehicle.Model,
                MaintenanceType = item.Maintenance.MaintenanceType.ToString(),
                Description = item.Maintenance.Description,
                StartDate = item.Maintenance.StartDate,
                EndDate = item.Maintenance.EndDate,
                NextInspectionDate = item.Maintenance.NextInspectionDate,
                Cost = item.Maintenance.Cost,
                Status = item.Maintenance.Status == MaintenanceStatus.Planned ? "Planlandı" :
                         item.Maintenance.Status == MaintenanceStatus.InProgress ? "Devam Ediyor" : "Tamamlandı",
                CreatedAt = item.Maintenance.CreatedAt
            });
        }

        /// <summary>POST /api/maintenance — Bakım kaydı ekle (Sadece Yönetici)</summary>
        [HttpPost]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            if (!Enum.TryParse<MaintenanceType>(dto.MaintenanceType, out var maintenanceType))
                return BadRequest("Geçersiz bakım tipi. Geçerli değerler: Periodic, Breakdown, Inspection, Other");

            var maintenance = new Maintenance
            {
                VehicleId = dto.VehicleId,
                MaintenanceType = maintenanceType,
                Description = dto.Description,
                StartDate = dto.StartDate.ToUniversalTime(),
                NextInspectionDate = dto.NextInspectionDate?.ToUniversalTime(),
                Cost = dto.Cost,
                Status = MaintenanceStatus.Planned,
                CreatedAt = DateTime.UtcNow
            };

            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bakım kaydı oluşturuldu.", maintenanceId = maintenance.MaintenanceId });
        }

        /// <summary>PUT /api/maintenance/{id} — Bakım durumunu güncelle (Sadece Yönetici)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaintenanceDto dto)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null) return NotFound("Bakım kaydı bulunamadı.");

            if (!Enum.TryParse<MaintenanceStatus>(dto.Status, out var status))
                return BadRequest("Geçersiz durum. Geçerli değerler: Planned, InProgress, Done");

            _context.Attach(maintenance);
            maintenance.Status = status;

            if (dto.EndDate.HasValue)
                maintenance.EndDate = dto.EndDate.Value.ToUniversalTime();
            if (dto.Cost.HasValue)
                maintenance.Cost = dto.Cost.Value;
            if (dto.NextInspectionDate.HasValue)
                maintenance.NextInspectionDate = dto.NextInspectionDate.Value.ToUniversalTime();

            await _context.SaveChangesAsync();

            return Ok(new { message = "Bakım kaydı güncellendi." });
        }
    }
}
