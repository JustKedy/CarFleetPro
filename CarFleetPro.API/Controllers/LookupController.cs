using CarFleetPro.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LookupController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Markaları Getir
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _context.CarBrands.OrderBy(b => b.Name).ToListAsync();
            return Ok(brands);
        }

        // 2. Seçilen Markanın Modellerini Getir (Yunus Marka seçince buraya o markanın ID'sini atacak)
        [HttpGet("models/{brandId}")]
        public async Task<IActionResult> GetModels(int brandId)
        {
            var models = await _context.CarModels
                .Where(m => m.BrandId == brandId)
                .OrderBy(m => m.Name)
                .ToListAsync();
            return Ok(models);
        }

        // 3. Renkleri Getir
        [HttpGet("colors")]
        public async Task<IActionResult> GetColors()
        {
            var colors = await _context.CarColors.OrderBy(c => c.Name).ToListAsync();
            return Ok(colors);
        }

        // 4. Mevcut Durumları Getir (Veritabanından değil, Enum'dan çekiyoruz ki sabit kalsın)
        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var statuses = new[]
            {
                new { Id = 0, Name = "Müsait" },
                new { Id = 1, Name = "Kirada" },
                new { Id = 2, Name = "Bakımda" }
            };
            return Ok(statuses);
        }
    }
}