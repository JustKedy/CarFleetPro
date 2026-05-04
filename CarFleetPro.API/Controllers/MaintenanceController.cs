using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    // Madde 5 — Araç Bakım ve Servis Controller
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

        // GET /api/maintenance — Tüm bakımlar (filtre: vehicleId, status)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? vehicleId,
            [FromQuery] string? status)
        {
            var query = _context.Maintenances
                .Join(_context.Vehicles, m => m.VehicleId, v => v.VehicleId,
                    (m, v) => new { m, v })
                .AsQueryable();

            if (vehicleId.HasValue)
                query = query.Where(x => x.m.VehicleId == vehicleId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "planlandı" || status.ToLower() == "planned")
                    query = query.Where(x => x.m.Status == MaintenanceStatus.Planned);
                else if (status.ToLower() == "devam ediyor" || status.ToLower() == "inprogress")
                    query = query.Where(x => x.m.Status == MaintenanceStatus.InProgress);
                else if (status.ToLower() == "tamamlandı" || status.ToLower() == "done")
                    query = query.Where(x => x.m.Status == MaintenanceStatus.Done);
            }

            var result = await query
                .OrderByDescending(x => x.m.CreatedAt)
                .Select(x => new MaintenanceListDto
                {
                    MaintenanceId = x.m.MaintenanceId,
                    VehicleId = x.m.VehicleId,
                    VehicleName = x.v.Brand + " " + x.v.Model,
                    PlateNumber = x.v.PlateNumber,
                    Description = x.m.Description,
                    MaintenanceType = x.m.MaintenanceType.ToString(),
                    StartDate = x.m.StartDate,
                    EndDate = x.m.EndDate,
                    NextInspectionDate = x.m.NextInspectionDate,
                    Cost = x.m.Cost,
                    Status = x.m.Status == MaintenanceStatus.Done ? "Tamamlandı" :
                             x.m.Status == MaintenanceStatus.InProgress ? "Devam Ediyor" : "Planlandı",
                    CreatedAt = x.m.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /api/maintenance/{id} — Tek bakım detayı
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Maintenances
                .Join(_context.Vehicles, m => m.VehicleId, v => v.VehicleId,
                    (m, v) => new { m, v })
                .FirstOrDefaultAsync(x => x.m.MaintenanceId == id);

            if (item == null) return NotFound("Bakım kaydı bulunamadı.");

            return Ok(new MaintenanceListDto
            {
                MaintenanceId = item.m.MaintenanceId,
                VehicleId = item.m.VehicleId,
                VehicleName = item.v.Brand + " " + item.v.Model,
                PlateNumber = item.v.PlateNumber,
                Description = item.m.Description,
                MaintenanceType = item.m.MaintenanceType.ToString(),
                StartDate = item.m.StartDate,
                EndDate = item.m.EndDate,
                NextInspectionDate = item.m.NextInspectionDate,
                Cost = item.m.Cost,
                Status = item.m.Status == MaintenanceStatus.Done ? "Tamamlandı" :
                         item.m.Status == MaintenanceStatus.InProgress ? "Devam Ediyor" : "Planlandı",
                CreatedAt = item.m.CreatedAt
            });
        }

        // POST /api/maintenance — Yeni bakım kaydı oluştur
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            var maintenance = new Maintenance
            {
                VehicleId = dto.VehicleId,
                Description = dto.Description,
                MaintenanceType = dto.MaintenanceType,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                NextInspectionDate = dto.NextInspectionDate.HasValue
                    ? dto.NextInspectionDate.Value.ToUniversalTime()
                    : null,
                Cost = dto.Cost,
                Status = MaintenanceStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            _context.Maintenances.Add(maintenance);

            // Araç durumunu bakımda olarak işaretle
            _context.Attach(vehicle);
            vehicle.Status = VehicleStatus.Maintenance;
            _context.Entry(vehicle).Property(v => v.Status).IsModified = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Bakım kaydı oluşturuldu.", maintenanceId = maintenance.MaintenanceId });
        }

        // PUT /api/maintenance/{id}/complete — Bakımı tamamla
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id, [FromQuery] DateTime? nextInspectionDate)
        {
            var maintenance = await _context.Maintenances.FirstOrDefaultAsync(m => m.MaintenanceId == id);
            if (maintenance == null) return NotFound("Bakım kaydı bulunamadı.");
            if (maintenance.Status == MaintenanceStatus.Done)
                return BadRequest("Bu bakım zaten tamamlanmış.");

            _context.Attach(maintenance);
            maintenance.Status = MaintenanceStatus.Done;
            maintenance.EndDate = DateTime.UtcNow;
            if (nextInspectionDate.HasValue)
                maintenance.NextInspectionDate = nextInspectionDate.Value.ToUniversalTime();

            _context.Entry(maintenance).State = EntityState.Modified;

            // Aracı müsait yap
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == maintenance.VehicleId);
            if (vehicle != null)
            {
                _context.Attach(vehicle);
                vehicle.Status = VehicleStatus.Available;
                _context.Entry(vehicle).Property(v => v.Status).IsModified = true;

                // Sonraki muayene tarihini de araca yaz
                if (nextInspectionDate.HasValue)
                {
                    vehicle.InspectionExpiry = nextInspectionDate.Value.ToUniversalTime();
                    _context.Entry(vehicle).Property(v => v.InspectionExpiry).IsModified = true;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Bakım tamamlandı, araç müsait duruma alındı." });
        }
    }
}
