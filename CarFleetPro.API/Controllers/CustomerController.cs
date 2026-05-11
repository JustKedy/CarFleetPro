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
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        
        
        
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var activeRentals = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Active)
                .Select(r => r.CustomerId)
                .ToListAsync();

            var rentalCounts = await _context.Rentals
                .GroupBy(r => r.CustomerId)
                .Select(g => new { CustomerId = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = customers.Select(c => new CustomerListDto
            {
                CustomerId = c.CustomerId,
                FullName = $"{c.FirstName} {c.LastName}",
                Initials = $"{(c.FirstName.Length > 0 ? c.FirstName[0] : '?')}{(c.LastName.Length > 0 ? c.LastName[0] : '?')}",
                PhoneNumber = c.PhoneNumber,
                HasActiveRental = activeRentals.Contains(c.CustomerId),
                RentalStatus = activeRentals.Contains(c.CustomerId) ? "Aktif Kirada" : "Müşteri",
                TotalRentals = rentalCounts.FirstOrDefault(r => r.CustomerId == c.CustomerId)?.Count ?? 0
            }).ToList();

            return Ok(result);
        }

        
        
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new List<CustomerListDto>());

            var searchTerm = q.ToLower();

            var customers = await _context.Customers
                .Where(c => (c.FirstName + " " + c.LastName).ToLower().Contains(searchTerm)
                         || c.PhoneNumber.Contains(searchTerm)
                         || c.IdentityNumber.Contains(searchTerm))
                .OrderBy(c => c.FirstName)
                .Take(20)
                .ToListAsync();

            var activeRentals = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Active)
                .Select(r => r.CustomerId)
                .ToListAsync();

            var result = customers.Select(c => new CustomerListDto
            {
                CustomerId = c.CustomerId,
                FullName = $"{c.FirstName} {c.LastName}",
                Initials = $"{(c.FirstName.Length > 0 ? c.FirstName[0] : '?')}{(c.LastName.Length > 0 ? c.LastName[0] : '?')}",
                PhoneNumber = c.PhoneNumber,
                HasActiveRental = activeRentals.Contains(c.CustomerId),
                RentalStatus = activeRentals.Contains(c.CustomerId) ? "Aktif Kirada" : "Müşteri"
            }).ToList();

            return Ok(result);
        }

        
        
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerDetail(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return NotFound("Müşteri bulunamadı.");

            var rentalHistory = await _context.Rentals
                .Where(r => r.CustomerId == id)
                .Join(_context.Vehicles,
                    r => r.VehicleId,
                    v => v.VehicleId,
                    (r, v) => new RentalHistoryDto
                    {
                        RentalId = r.RentalId,
                        VehicleName = v.Brand + " " + v.Model,
                        PlateNumber = v.PlateNumber,
                        StartDate = r.StartDate,
                        PlannedEndDate = r.PlannedEndDate,
                        TotalAmount = r.TotalAmount,
                        Status = r.Status == RentalStatus.Active ? "Aktif" :
                                 r.Status == RentalStatus.Completed ? "Tamamlandı" : "İptal"
                    })
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            var detail = new CustomerDetailDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                IdentityNumber = customer.IdentityNumber,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                DateOfBirth = customer.DateOfBirth,
                DriverLicenseNumber = customer.DriverLicenseNumber,
                DriverLicenseExpiry = customer.DriverLicenseExpiry,
                Address = customer.Address,
                IsBlacklisted = customer.IsBlacklisted,
                RentalHistory = rentalHistory
            };

            return Ok(detail);
        }

        
        
        
        [HttpGet("names")]
        public async Task<IActionResult> GetCustomerNames()
        {
            var names = await _context.Customers
                .OrderBy(c => c.FirstName)
                .Select(c => new { c.CustomerId, FullName = c.FirstName + " " + c.LastName })
                .ToListAsync();

            return Ok(names);
        }

        [HttpPost("guest")]
        public async Task<IActionResult> AddGuestCustomer([FromBody] CreateCustomerDto dto)
        {
            // Aynı telefon numarasıyla kayıt varsa mevcut ID'yi döndür
            var existing = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == dto.PhoneNumber);

            if (existing != null)
                return Ok(new { customerId = existing.CustomerId });

            var customer = new Customer
            {
                FirstName           = dto.FirstName,
                LastName            = dto.LastName,
                IdentityNumber      = string.IsNullOrWhiteSpace(dto.IdentityNumber)
                                        ? $"MISAFIR{dto.PhoneNumber.TakeLast(10).Aggregate("", (a, c2) => a + c2)}"
                                        : dto.IdentityNumber,
                Email               = string.IsNullOrWhiteSpace(dto.Email)
                                        ? $"misafir_{dto.PhoneNumber.Replace(" ", "")}@carfleetpro.com"
                                        : dto.Email,
                PhoneNumber         = dto.PhoneNumber,
                DateOfBirth         = dto.DateOfBirth == default ? new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc) : dto.DateOfBirth.ToUniversalTime(),
                DriverLicenseNumber = string.IsNullOrWhiteSpace(dto.DriverLicenseNumber)
                                        ? $"GS{dto.PhoneNumber.TakeLast(6).Aggregate("", (a, c2) => a + c2)}"
                                        : dto.DriverLicenseNumber,
                DriverLicenseExpiry = dto.DriverLicenseExpiry == default ? DateTime.UtcNow.AddYears(5) : dto.DriverLicenseExpiry.ToUniversalTime(),
                Address             = string.IsNullOrWhiteSpace(dto.Address) ? "Belirtilmedi" : dto.Address,
                CreatedAt           = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { customerId = customer.CustomerId });
        }

        [HttpPost]

        public async Task<IActionResult> AddCustomer([FromBody] CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IdentityNumber = dto.IdentityNumber,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth.ToUniversalTime(),
                DriverLicenseNumber = dto.DriverLicenseNumber,
                DriverLicenseExpiry = dto.DriverLicenseExpiry.ToUniversalTime(),
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Müşteri sisteme başarıyla kaydedildi!", customerId = customer.CustomerId });
        }
    }
}