using CarFleetPro.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        
        private const string BrandsCacheKey = "lookup_brands";
        private const string ColorsCacheKey = "lookup_colors";
        private const string ModelsCacheKey = "lookup_models_"; 
        private static readonly TimeSpan LookupCacheDuration = TimeSpan.FromHours(24);

        public LookupController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            if (!_cache.TryGetValue(BrandsCacheKey, out object? brands) || brands == null)
            {
                brands = await _context.CarBrands.OrderBy(b => b.Name).ToListAsync();
                _cache.Set(BrandsCacheKey, brands, LookupCacheDuration);
            }
            return Ok(brands);
        }

        
        [HttpGet("models/{brandId:int}")]
        public async Task<IActionResult> GetModelsByBrandId(int brandId)
        {
            var cacheKey = ModelsCacheKey + brandId;
            if (!_cache.TryGetValue(cacheKey, out object? models) || models == null)
            {
                models = await _context.CarModels
                    .Where(m => m.BrandId == brandId)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
                _cache.Set(cacheKey, models, LookupCacheDuration);
            }
            return Ok(models);
        }

        
        [HttpGet("colors")]
        public async Task<IActionResult> GetColors()
        {
            if (!_cache.TryGetValue(ColorsCacheKey, out object? colors) || colors == null)
            {
                colors = await _context.CarColors.OrderBy(c => c.Name).ToListAsync();
                _cache.Set(ColorsCacheKey, colors, LookupCacheDuration);
            }
            return Ok(colors);
        }

        
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
    
    
    

    
    
    
    [Route("api/CarBrands")]
    [ApiController]
    public class CarBrandsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "alias_brands_objects";

        public CarBrandsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out object? brands) || brands == null)
            {
                brands = await _context.CarBrands.OrderBy(b => b.Name).ToListAsync();
                _cache.Set(CacheKey, brands, TimeSpan.FromHours(24));
            }
            return Ok(brands);
        }
    }

    [Route("api/CarModels")]
    [ApiController]
    public class CarModelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public CarModelsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("{brandId}")]
        public async Task<IActionResult> GetByBrandId(int brandId)
        {
            var cacheKey = $"alias_models_obj_{brandId}";

            if (!_cache.TryGetValue(cacheKey, out object? models) || models == null)
            {
                models = await _context.CarModels
                    .Where(m => m.BrandId == brandId)
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                _cache.Set(cacheKey, models, TimeSpan.FromHours(24));
            }
            return Ok(models);
        }
    }

    [Route("api/CarColors")]
    [ApiController]
    public class CarColorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "alias_colors_objects";

        public CarColorsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out object? colors) || colors == null)
            {
                colors = await _context.CarColors.OrderBy(c => c.Name).ToListAsync();
                _cache.Set(CacheKey, colors, TimeSpan.FromHours(24));
            }
            return Ok(colors);
        }
    }

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

    [Route("api/CarTypes")]
    [ApiController]
    public class CarTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "alias_types_objects";

        public CarTypesController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out object? types) || types == null)
            {
                types = await _context.CarTypes.OrderBy(c => c.Name).ToListAsync();
                _cache.Set(CacheKey, types, TimeSpan.FromHours(24));
            }
            return Ok(types);
        }
    }
}