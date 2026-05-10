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
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/invoice — Fatura listesi (Admin + Çalışan görüntüleyebilir)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _context.Invoices
                .Join(_context.Rentals, i => i.RentalId, r => r.RentalId,
                    (i, r) => new { Invoice = i, Rental = r })
                .Join(_context.Customers, ir => ir.Rental.CustomerId, c => c.CustomerId,
                    (ir, c) => new { ir.Invoice, ir.Rental, Customer = c })
                .Join(_context.Vehicles, irc => irc.Rental.VehicleId, v => v.VehicleId,
                    (irc, v) => new { irc.Invoice, irc.Rental, irc.Customer, Vehicle = v })
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "pending")
                    query = query.Where(x => x.Invoice.Status == InvoiceStatus.Pending);
                else if (status.ToLower() == "paid")
                    query = query.Where(x => x.Invoice.Status == InvoiceStatus.Paid);
                else if (status.ToLower() == "cancelled")
                    query = query.Where(x => x.Invoice.Status == InvoiceStatus.Cancelled);
            }

            var result = await query
                .OrderByDescending(x => x.Invoice.IssuedAt)
                .Select(x => new InvoiceDto
                {
                    InvoiceId = x.Invoice.InvoiceId,
                    RentalId = x.Invoice.RentalId,
                    Amount = x.Invoice.Amount,
                    IssuedAt = x.Invoice.IssuedAt,
                    PaidAt = x.Invoice.PaidAt,
                    Status = x.Invoice.Status == InvoiceStatus.Pending ? "Bekliyor" :
                             x.Invoice.Status == InvoiceStatus.Paid ? "Ödendi" : "İptal",
                    Notes = x.Invoice.Notes,
                    CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                    VehiclePlate = x.Vehicle.PlateNumber
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>GET /api/invoice/{id} — Fatura detayı</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Invoices
                .Join(_context.Rentals, i => i.RentalId, r => r.RentalId,
                    (i, r) => new { Invoice = i, Rental = r })
                .Join(_context.Customers, ir => ir.Rental.CustomerId, c => c.CustomerId,
                    (ir, c) => new { ir.Invoice, ir.Rental, Customer = c })
                .Join(_context.Vehicles, irc => irc.Rental.VehicleId, v => v.VehicleId,
                    (irc, v) => new { irc.Invoice, irc.Rental, irc.Customer, Vehicle = v })
                .FirstOrDefaultAsync(x => x.Invoice.InvoiceId == id);

            if (item == null) return NotFound("Fatura bulunamadı.");

            return Ok(new InvoiceDto
            {
                InvoiceId = item.Invoice.InvoiceId,
                RentalId = item.Invoice.RentalId,
                Amount = item.Invoice.Amount,
                IssuedAt = item.Invoice.IssuedAt,
                PaidAt = item.Invoice.PaidAt,
                Status = item.Invoice.Status == InvoiceStatus.Pending ? "Bekliyor" :
                         item.Invoice.Status == InvoiceStatus.Paid ? "Ödendi" : "İptal",
                Notes = item.Invoice.Notes,
                CustomerName = item.Customer.FirstName + " " + item.Customer.LastName,
                VehiclePlate = item.Vehicle.PlateNumber
            });
        }

        /// <summary>POST /api/invoice — Fatura oluştur (Sadece Yönetici)</summary>
        [HttpPost]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
        {
            var rental = await _context.Rentals.FindAsync(dto.RentalId);
            if (rental == null) return NotFound("Kiralama bulunamadı.");

            var invoice = new Invoice
            {
                RentalId = dto.RentalId,
                Amount = dto.Amount,
                Status = InvoiceStatus.Pending,
                IssuedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura oluşturuldu.", invoiceId = invoice.InvoiceId });
        }

        /// <summary>PUT /api/invoice/{id} — Fatura durumunu güncelle (Sadece Yönetici)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceDto dto)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound("Fatura bulunamadı.");

            if (!Enum.TryParse<InvoiceStatus>(dto.Status, out var newStatus))
                return BadRequest("Geçersiz durum. Geçerli değerler: Pending, Paid, Cancelled");

            _context.Attach(invoice);
            invoice.Status = newStatus;
            invoice.Notes = dto.Notes ?? invoice.Notes;

            if (newStatus == InvoiceStatus.Paid)
                invoice.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura güncellendi." });
        }
    }
}
