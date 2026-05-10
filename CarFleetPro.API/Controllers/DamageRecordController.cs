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
    public class DamageRecordController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DamageRecordController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/damagerecord — Hasar kayıtları (Admin + Çalışan)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? vehicleId, [FromQuery] string? status)
        {
            var query = _context.DamageRecords
                .Join(_context.Vehicles, d => d.VehicleId, v => v.VehicleId,
                    (d, v) => new { Damage = d, Vehicle = v })
                .AsQueryable();

            if (vehicleId.HasValue)
                query = query.Where(x => x.Vehicle.VehicleId == vehicleId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "pending")
                    query = query.Where(x => x.Damage.Status == DamageRecordStatus.Pending);
                else if (status.ToLower() == "underrepair")
                    query = query.Where(x => x.Damage.Status == DamageRecordStatus.UnderRepair);
                else if (status.ToLower() == "completed")
                    query = query.Where(x => x.Damage.Status == DamageRecordStatus.Completed);
            }

            var result = await query
                .OrderByDescending(x => x.Damage.CreatedAt)
                .Select(x => new DamageRecordDto
                {
                    DamageRecordId = x.Damage.DamageRecordId,
                    VehicleId = x.Vehicle.VehicleId,
                    VehiclePlate = x.Vehicle.PlateNumber,
                    VehicleName = x.Vehicle.Brand + " " + x.Vehicle.Model,
                    DamageType = x.Damage.DamageType.ToString(),
                    Description = x.Damage.Description,
                    DamageDate = x.Damage.DamageDate,
                    EstimatedCost = x.Damage.EstimatedCost,
                    Status = x.Damage.Status == DamageRecordStatus.Pending ? "İşlem Bekliyor" :
                             x.Damage.Status == DamageRecordStatus.UnderRepair ? "Onarımda" : "Tamamlandı",
                    PhotoUrl = x.Damage.PhotoUrl,
                    CreatedAt = x.Damage.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>GET /api/damagerecord/{id} — Hasar detayı</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.DamageRecords
                .Join(_context.Vehicles, d => d.VehicleId, v => v.VehicleId,
                    (d, v) => new { Damage = d, Vehicle = v })
                .FirstOrDefaultAsync(x => x.Damage.DamageRecordId == id);

            if (item == null) return NotFound("Hasar kaydı bulunamadı.");

            return Ok(new DamageRecordDto
            {
                DamageRecordId = item.Damage.DamageRecordId,
                VehicleId = item.Vehicle.VehicleId,
                VehiclePlate = item.Vehicle.PlateNumber,
                VehicleName = item.Vehicle.Brand + " " + item.Vehicle.Model,
                DamageType = item.Damage.DamageType.ToString(),
                Description = item.Damage.Description,
                DamageDate = item.Damage.DamageDate,
                EstimatedCost = item.Damage.EstimatedCost,
                Status = item.Damage.Status == DamageRecordStatus.Pending ? "İşlem Bekliyor" :
                         item.Damage.Status == DamageRecordStatus.UnderRepair ? "Onarımda" : "Tamamlandı",
                PhotoUrl = item.Damage.PhotoUrl,
                CreatedAt = item.Damage.CreatedAt
            });
        }

        /// <summary>POST /api/damagerecord — Hasar kaydı ekle (Admin + Çalışan ekleyebilir)</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDamageRecordDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            if (!Enum.TryParse<DamageType>(dto.DamageType, out var damageType))
                return BadRequest("Geçersiz hasar tipi. Geçerli değerler: Body, Mechanical, Glass, Interior, Other");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var record = new DamageRecord
            {
                VehicleId = dto.VehicleId,
                ReportedByUserId = userId,
                DamageType = damageType,
                Description = dto.Description,
                DamageDate = dto.DamageDate.ToUniversalTime(),
                EstimatedCost = dto.EstimatedCost,
                PhotoUrl = dto.PhotoUrl,
                Status = DamageRecordStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.DamageRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hasar kaydı oluşturuldu.", damageRecordId = record.DamageRecordId });
        }

        /// <summary>PUT /api/damagerecord/{id} — Hasar kaydını güncelle (Sadece Yönetici)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDamageRecordDto dto)
        {
            var record = await _context.DamageRecords.FindAsync(id);
            if (record == null) return NotFound("Hasar kaydı bulunamadı.");

            if (!Enum.TryParse<DamageRecordStatus>(dto.Status, out var status))
                return BadRequest("Geçersiz durum. Geçerli değerler: Pending, UnderRepair, Completed");

            _context.Attach(record);
            record.Status = status;
            if (dto.EstimatedCost.HasValue)
                record.EstimatedCost = dto.EstimatedCost.Value;
            if (!string.IsNullOrEmpty(dto.PhotoUrl))
                record.PhotoUrl = dto.PhotoUrl;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Hasar kaydı güncellendi." });
        }
    }
}
