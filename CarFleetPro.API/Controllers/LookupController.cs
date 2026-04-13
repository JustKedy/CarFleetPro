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

        // 1. Markaları Getir (ID + Name nesnesi döndürür)
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _context.CarBrands.OrderBy(b => b.Name).ToListAsync();
            return Ok(brands);
        }

        // 2. Seçilen Markanın Modellerini Getir (brandId ile)
        [HttpGet("models/{brandId:int}")]
        public async Task<IActionResult> GetModelsByBrandId(int brandId)
        {
            var models = await _context.CarModels
                .Where(m => m.BrandId == brandId)
                .OrderBy(m => m.Name)
                .ToListAsync();
            return Ok(models);
        }

        // 3. Renkleri Getir (ID + Name nesnesi döndürür)
        [HttpGet("colors")]
        public async Task<IActionResult> GetColors()
        {
            var colors = await _context.CarColors.OrderBy(c => c.Name).ToListAsync();
            return Ok(colors);
        }

        // 4. Mevcut Durumları Getir (Enum'dan sabit liste)
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

namespace CarFleetPro.API.Controllers
{
    // ======================================================
    // === MOBİL UYGULAMA İÇİN ALIAS CONTROLLER'LAR ========
    // ======================================================

    /// <summary>
    /// GET api/CarBrands → Tüm markaları döndürür (string listesi)
    /// </summary>
    [Route("api/CarBrands")]
    [ApiController]
    public class CarBrandsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CarBrandsController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var brands = await _context.CarBrands.OrderBy(b => b.Name).ToListAsync();
            return Ok(brands.Select(b => b.Name).ToList());
        }
    }

    /// <summary>
    /// GET api/CarModels/{brandName} → Marka adına göre modelleri döndürür (string listesi)
    /// </summary>
    [Route("api/CarModels")]
    [ApiController]
    public class CarModelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CarModelsController(AppDbContext context) { _context = context; }

        [HttpGet("{brandName}")]
        public async Task<IActionResult> GetByBrandName(string brandName)
        {
            var brand = await _context.CarBrands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == brandName.ToLower());

            if (brand == null) return Ok(new List<string>());

            var models = await _context.CarModels
                .Where(m => m.BrandId == brand.Id)
                .OrderBy(m => m.Name)
                .Select(m => m.Name)
                .ToListAsync();

            return Ok(models);
        }
    }

    /// <summary>
    /// GET api/CarColors → Tüm renkleri döndürür (string listesi)
    /// </summary>
    [Route("api/CarColors")]
    [ApiController]
    public class CarColorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CarColorsController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var colors = await _context.CarColors.OrderBy(c => c.Name).ToListAsync();
            return Ok(colors.Select(c => c.Name).ToList());
        }
    }

    /// <summary>
    /// GET api/VehicleStatuses → Araç durum listesini döndürür (string listesi)
    /// </summary>
    [Route("api/VehicleStatuses")]
    [ApiController]
    public class VehicleStatusesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var statuses = new List<string> { "MÜSAİT", "DOLU", "BAKIMDA" };
            return Ok(statuses);
        }
    }
}