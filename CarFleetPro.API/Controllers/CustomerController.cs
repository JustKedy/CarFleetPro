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

        /// <summary>
        /// GET /api/Customer — Tüm müşterileri listele
        /// </summary>
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

        /// <summary>
        /// GET /api/Customer/search?q=isim veya telefon
        /// </summary>
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

        /// <summary>
        /// GET /api/Customer/{id} — Müşteri detayı + kiralama geçmişi
        /// </summary>
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

        /// <summary>
        /// GET /api/Customer/names — Kiralama formu için müşteri id+isim listesi
        /// </summary>
        [HttpGet("names")]
        public async Task<IActionResult> GetCustomerNames()
        {
            var names = await _context.Customers
                .OrderBy(c => c.FirstName)
                .Select(c => new { c.CustomerId, FullName = c.FirstName + " " + c.LastName })
                .ToListAsync();

            return Ok(names);
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