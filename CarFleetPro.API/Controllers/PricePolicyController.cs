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

            await _context.SaveChangesAsync();
            
            // Invalidate vehicle caches so pricing updates reflect immediately
            _cache.Remove("vehicleList");
            _cache.Remove("vehicleCards");
            _cache.Remove("vehicleCardsETag");

            return Ok(request);
        }
    }
}
