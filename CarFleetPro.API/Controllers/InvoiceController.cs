using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    // Madde 4 — Ödeme ve Faturalar Controller
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

        // GET /api/invoice — Tüm faturalar (filtre: rentalId, status)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? rentalId,
            [FromQuery] string? status)
        {
            var query = _context.Invoices
                .Join(_context.Rentals, i => i.RentalId, r => r.RentalId,
                    (i, r) => new { i, r })
                .Join(_context.Customers, ir => ir.r.CustomerId, c => c.CustomerId,
                    (ir, c) => new { ir.i, ir.r, c })
                .Join(_context.Vehicles, irc => irc.r.VehicleId, v => v.VehicleId,
                    (irc, v) => new { irc.i, irc.r, irc.c, v })
                .AsQueryable();

            if (rentalId.HasValue)
                query = query.Where(x => x.i.RentalId == rentalId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "bekliyor")
                    query = query.Where(x => x.i.Status == InvoiceStatus.Bekliyor);
                else if (status.ToLower() == "odendi" || status.ToLower() == "ödendi")
                    query = query.Where(x => x.i.Status == InvoiceStatus.Odendi);
                else if (status.ToLower() == "iptal")
                    query = query.Where(x => x.i.Status == InvoiceStatus.Iptal);
            }

            var result = await query
                .OrderByDescending(x => x.i.CreatedAt)
                .Select(x => new InvoiceListDto
                {
                    InvoiceId = x.i.InvoiceId,
                    RentalId = x.i.RentalId,
                    CustomerName = x.c.FirstName + " " + x.c.LastName,
                    VehicleName = x.v.Brand + " " + x.v.Model,
                    TahsilatTarihi = x.i.TahsilatTarihi,
                    Tutar = x.i.Tutar,
                    OdemeYontemi = x.i.OdemYontemi.ToString(),
                    Status = x.i.Status == InvoiceStatus.Bekliyor ? "Bekliyor" :
                             x.i.Status == InvoiceStatus.Odendi ? "Ödendi" : "İptal",
                    Notes = x.i.Notes,
                    CreatedAt = x.i.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /api/invoice/{id} — Tek fatura
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Invoices
                .Join(_context.Rentals, i => i.RentalId, r => r.RentalId,
                    (i, r) => new { i, r })
                .Join(_context.Customers, ir => ir.r.CustomerId, c => c.CustomerId,
                    (ir, c) => new { ir.i, ir.r, c })
                .Join(_context.Vehicles, irc => irc.r.VehicleId, v => v.VehicleId,
                    (irc, v) => new { irc.i, irc.r, irc.c, v })
                .FirstOrDefaultAsync(x => x.i.InvoiceId == id);

            if (item == null) return NotFound("Fatura bulunamadı.");

            return Ok(new InvoiceListDto
            {
                InvoiceId = item.i.InvoiceId,
                RentalId = item.i.RentalId,
                CustomerName = item.c.FirstName + " " + item.c.LastName,
                VehicleName = item.v.Brand + " " + item.v.Model,
                TahsilatTarihi = item.i.TahsilatTarihi,
                Tutar = item.i.Tutar,
                OdemeYontemi = item.i.OdemYontemi.ToString(),
                Status = item.i.Status == InvoiceStatus.Bekliyor ? "Bekliyor" :
                         item.i.Status == InvoiceStatus.Odendi ? "Ödendi" : "İptal",
                Notes = item.i.Notes,
                CreatedAt = item.i.CreatedAt
            });
        }

        // POST /api/invoice — Yeni fatura oluştur
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == dto.RentalId);
            if (rental == null) return NotFound("Kiralama kaydı bulunamadı.");

            var invoice = new Invoice
            {
                RentalId = dto.RentalId,
                TahsilatTarihi = dto.TahsilatTarihi.ToUniversalTime(),
                Tutar = dto.Tutar,
                OdemYontemi = dto.OdemeYontemi,
                Status = InvoiceStatus.Bekliyor,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura oluşturuldu.", invoiceId = invoice.InvoiceId });
        }

        // PUT /api/invoice/{id}/pay — Faturayı ödenmiş işaretle
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null) return NotFound("Fatura bulunamadı.");
            if (invoice.Status == InvoiceStatus.Odendi)
                return BadRequest("Bu fatura zaten ödenmiş.");

            _context.Attach(invoice);
            invoice.Status = InvoiceStatus.Odendi;
            _context.Entry(invoice).Property(i => i.Status).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura ödendi olarak işaretlendi." });
        }

        // PUT /api/invoice/{id}/cancel — Faturayı iptal et
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelInvoice(int id)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null) return NotFound("Fatura bulunamadı.");
            if (invoice.Status == InvoiceStatus.Iptal)
                return BadRequest("Bu fatura zaten iptal edilmiş.");

            _context.Attach(invoice);
            invoice.Status = InvoiceStatus.Iptal;
            _context.Entry(invoice).Property(i => i.Status).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura iptal edildi." });
        }
    }
}
