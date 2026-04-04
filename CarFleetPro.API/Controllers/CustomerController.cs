using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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