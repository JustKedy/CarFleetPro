using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    // Madde 6 — Hasar Kayıtları Controller
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DamageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DamageController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/damage — Tüm hasar kayıtları (filtre: vehicleId, status)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? vehicleId,
            [FromQuery] string? status)
        {
            var query = _context.DamageRecords
                .Join(_context.Vehicles, d => d.VehicleId, v => v.VehicleId,
                    (d, v) => new { d, v })
                .AsQueryable();

            if (vehicleId.HasValue)
                query = query.Where(x => x.d.VehicleId == vehicleId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "islembekliyor" || status.ToLower() == "işlem bekliyor")
                    query = query.Where(x => x.d.Status == DamageStatus.IslemBekliyor);
                else if (status.ToLower() == "onarimda" || status.ToLower() == "onarımda")
                    query = query.Where(x => x.d.Status == DamageStatus.Onarimda);
                else if (status.ToLower() == "tamamlandi" || status.ToLower() == "tamamlandı")
                    query = query.Where(x => x.d.Status == DamageStatus.Tamamlandi);
            }

            var result = await query
                .OrderByDescending(x => x.d.CreatedAt)
                .Select(x => new DamageListDto
                {
                    DamageId = x.d.DamageId,
                    VehicleId = x.d.VehicleId,
                    VehicleName = x.v.Brand + " " + x.v.Model,
                    PlateNumber = x.v.PlateNumber,
                    DamageType = x.d.DamageType,
                    Date = x.d.Date,
                    EstimatedCost = x.d.EstimatedCost,
                    Status = x.d.Status == DamageStatus.IslemBekliyor ? "İşlem Bekliyor" :
                             x.d.Status == DamageStatus.Onarimda ? "Onarımda" : "Tamamlandı",
                    PhotoUrl = x.d.PhotoUrl,
                    Notes = x.d.Notes,
                    CreatedAt = x.d.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /api/damage/{id} — Tek hasar detayı
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.DamageRecords
                .Join(_context.Vehicles, d => d.VehicleId, v => v.VehicleId,
                    (d, v) => new { d, v })
                .FirstOrDefaultAsync(x => x.d.DamageId == id);

            if (item == null) return NotFound("Hasar kaydı bulunamadı.");

            return Ok(new DamageListDto
            {
                DamageId = item.d.DamageId,
                VehicleId = item.d.VehicleId,
                VehicleName = item.v.Brand + " " + item.v.Model,
                PlateNumber = item.v.PlateNumber,
                DamageType = item.d.DamageType,
                Date = item.d.Date,
                EstimatedCost = item.d.EstimatedCost,
                Status = item.d.Status == DamageStatus.IslemBekliyor ? "İşlem Bekliyor" :
                         item.d.Status == DamageStatus.Onarimda ? "Onarımda" : "Tamamlandı",
                PhotoUrl = item.d.PhotoUrl,
                Notes = item.d.Notes,
                CreatedAt = item.d.CreatedAt
            });
        }

        // POST /api/damage — Yeni hasar kaydı
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDamageDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);
            if (vehicle == null) return NotFound("Araç bulunamadı.");

            var damage = new DamageRecord
            {
                VehicleId = dto.VehicleId,
                DamageType = dto.DamageType,
                Date = dto.Date.ToUniversalTime(),
                EstimatedCost = dto.EstimatedCost,
                Status = DamageStatus.IslemBekliyor,
                PhotoUrl = dto.PhotoUrl,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.DamageRecords.Add(damage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hasar kaydı oluşturuldu.", damageId = damage.DamageId });
        }

        // PUT /api/damage/{id}/status — Hasar durumunu güncelle
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDamageStatusDto dto)
        {
            var damage = await _context.DamageRecords.FirstOrDefaultAsync(d => d.DamageId == id);
            if (damage == null) return NotFound("Hasar kaydı bulunamadı.");

            _context.Attach(damage);
            damage.Status = dto.Status;
            _context.Entry(damage).Property(d => d.Status).IsModified = true;
            await _context.SaveChangesAsync();

            var statusText = dto.Status == DamageStatus.IslemBekliyor ? "İşlem Bekliyor" :
                             dto.Status == DamageStatus.Onarimda ? "Onarımda" : "Tamamlandı";

            return Ok(new { message = $"Hasar durumu '{statusText}' olarak güncellendi." });
        }
    }
}
