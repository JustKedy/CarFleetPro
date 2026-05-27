using CarFleetPro.API.Data;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Memory;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PricePolicyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PricePolicyController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PricePolicy>>> GetPolicies()
        {
            return await _context.PricePolicies.ToListAsync();
        }

        [HttpGet("{type}/{value}")]
        public async Task<ActionResult<PricePolicy>> GetPolicy(string type, string value)
        {
            var policy = await _context.PricePolicies
                .FirstOrDefaultAsync(p => p.TargetType == type && p.TargetValue == value);

            if (policy == null) return NotFound();
            return policy;
        }

        [HttpPost]
        public async Task<ActionResult<PricePolicy>> SavePolicy(PricePolicy request)
        {
            var existing = await _context.PricePolicies
                .FirstOrDefaultAsync(p => p.TargetType == request.TargetType && p.TargetValue == request.TargetValue);

            if (existing != null)
            {
                existing.BasePrice = request.BasePrice;
                existing.MaxDiscountPercentage = request.MaxDiscountPercentage;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.PricePolicies.Update(existing);
            }
            else
            {
                _context.PricePolicies.Add(request);
            }

            IQueryable<Vehicle> vehiclesQuery = _context.Vehicles;

            if (request.TargetType == "Segment")
            {
                vehiclesQuery = vehiclesQuery.Include(v => v.Type)
                                             .Where(v => v.Type.Name == request.TargetValue);
            }
            else if (request.TargetType == "Brand")
            {
                vehiclesQuery = vehiclesQuery.Include(v => v.Brand)
                                             .Where(v => v.Brand.Name == request.TargetValue);
            }

            var vehiclesToUpdate = await vehiclesQuery.ToListAsync();

            foreach (var vehicle in vehiclesToUpdate)
            {
                vehicle.BasePrice = request.BasePrice;
                vehicle.MaxDiscountPercentage = request.MaxDiscountPercentage;
                _context.Vehicles.Update(vehicle);
            }

            await _context.SaveChangesAsync();
            
            _cache.Remove("vehicleList");
            _cache.Remove("vehicleCards");
            _cache.Remove("vehicleCardsETag");

            return Ok(request);
        }
    }
}
