using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        /// <summary>GET /api/invoice</summary>
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
                    InvoiceId    = x.Invoice.InvoiceId,
                    RentalId     = x.Invoice.RentalId,
                    Amount       = x.Invoice.Amount,
                    IssuedAt     = x.Invoice.IssuedAt,
                    PaidAt       = x.Invoice.PaidAt,
                    Status       = x.Invoice.Status == InvoiceStatus.Pending ? "Bekliyor" :
                                   x.Invoice.Status == InvoiceStatus.Paid    ? "Ödendi"   : "İptal",
                    Notes        = x.Invoice.Notes,
                    CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                    VehiclePlate = x.Vehicle.PlateNumber
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>GET /api/invoice/{id}</summary>
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
                InvoiceId    = item.Invoice.InvoiceId,
                RentalId     = item.Invoice.RentalId,
                Amount       = item.Invoice.Amount,
                IssuedAt     = item.Invoice.IssuedAt,
                PaidAt       = item.Invoice.PaidAt,
                Status       = item.Invoice.Status == InvoiceStatus.Pending ? "Bekliyor" :
                               item.Invoice.Status == InvoiceStatus.Paid    ? "Ödendi"   : "İptal",
                Notes        = item.Invoice.Notes,
                CustomerName = item.Customer.FirstName + " " + item.Customer.LastName,
                VehiclePlate = item.Vehicle.PlateNumber
            });
        }

        /// <summary>POST /api/invoice — Yönetici</summary>
        [HttpPost]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
        {
            var rental = await _context.Rentals.FindAsync(dto.RentalId);
            if (rental == null) return NotFound("Kiralama bulunamadı.");

            var invoice = new Invoice
            {
                RentalId = dto.RentalId,
                Amount   = dto.Amount,
                Status   = InvoiceStatus.Pending,
                IssuedAt = DateTime.UtcNow,
                Notes    = dto.Notes
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura oluşturuldu.", invoiceId = invoice.InvoiceId });
        }

        /// <summary>PUT /api/invoice/{id} — Yönetici</summary>
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
            invoice.Notes  = dto.Notes ?? invoice.Notes;

            if (newStatus == InvoiceStatus.Paid)
                invoice.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Fatura güncellendi." });
        }

        // ═══════════════════════════════════════════════════════════════════
        // GET /api/invoice/{id}/pdf  — İndirilebilir PDF fatura
        // ═══════════════════════════════════════════════════════════════════
        [HttpGet("{id}/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPdf(int id)
        {
            var item = await _context.Invoices
                .Join(_context.Rentals, i => i.RentalId, r => r.RentalId,
                    (i, r) => new { Invoice = i, Rental = r })
                .Join(_context.Customers, ir => ir.Rental.CustomerId, c => c.CustomerId,
                    (ir, c) => new { ir.Invoice, ir.Rental, Customer = c })
                .Join(_context.Vehicles.Include(v => v.Brand).Include(v => v.Model), irc => irc.Rental.VehicleId, v => v.VehicleId,
                    (irc, v) => new { irc.Invoice, irc.Rental, irc.Customer, Vehicle = v })
                .FirstOrDefaultAsync(x => x.Invoice.InvoiceId == id);

            if (item == null) return NotFound("Fatura bulunamadı.");

            var data = new InvoicePdfData
            {
                InvoiceId       = item.Invoice.InvoiceId,
                IssuedAt        = item.Invoice.IssuedAt,
                Amount          = item.Invoice.Amount,
                Status          = item.Invoice.Status,
                Notes           = item.Invoice.Notes ?? "",
                StartDate       = item.Rental.StartDate,
                EndDate         = item.Rental.PlannedEndDate,
                DailyRate       = item.Rental.DailyRate,
                CustomerName    = $"{item.Customer.FirstName} {item.Customer.LastName}",
                CustomerPhone   = item.Customer.PhoneNumber,
                CustomerEmail   = item.Customer.Email,
                CustomerAddress = item.Customer.Address,
                VehicleBrand    = item.Vehicle.Brand.Name,
                VehicleModel    = item.Vehicle.Model.Name,
                VehiclePlate    = item.Vehicle.PlateNumber,
            };

            var pdfBytes = BuildPdf(data);
            return File(pdfBytes, "application/pdf", $"Fatura_INV-{id:D5}.pdf");
        }

        // ── Yardımcı: strongly-typed data transfer record ────────────────────
        private sealed record InvoicePdfData
        {
            public int           InvoiceId       { get; init; }
            public DateTime      IssuedAt        { get; init; }
            public decimal       Amount          { get; init; }
            public InvoiceStatus Status          { get; init; }
            public string        Notes           { get; init; } = "";
            public DateTime      StartDate       { get; init; }
            public DateTime      EndDate         { get; init; }
            public decimal       DailyRate       { get; init; }
            public string        CustomerName    { get; init; } = "";
            public string        CustomerPhone   { get; init; } = "";
            public string        CustomerEmail   { get; init; } = "";
            public string        CustomerAddress { get; init; } = "";
            public string        VehicleBrand    { get; init; } = "";
            public string        VehicleModel    { get; init; } = "";
            public string        VehiclePlate    { get; init; } = "";
        }

        private static byte[] BuildPdf(InvoicePdfData d)
        {
            int days = Math.Max(1, (d.EndDate - d.StartDate).Days);
            decimal kdv   = Math.Round(d.Amount * 0.18m, 2);
            decimal total = d.Amount + kdv;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ── HEADER ──────────────────────────────────────────────────
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CARFLEET PRO")
                               .FontSize(26).Bold().FontColor("#1D4ED8");
                            col.Item().Text("Araç Kiralama & Filo Yönetimi")
                               .FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        row.ConstantItem(160).AlignRight().Column(col =>
                        {
                            col.Item().Text("FATURA").FontSize(20).Bold();
                            col.Item().Text($"No: INV-{d.InvoiceId:D5}").Bold();
                            col.Item().Text($"Tarih: {d.IssuedAt:dd.MM.yyyy}");

                            string statusTxt   = d.Status == InvoiceStatus.Paid       ? "ÖDENDİ"  :
                                                 d.Status == InvoiceStatus.Cancelled  ? "İPTAL"   : "BEKLİYOR";
                            string statusColor = d.Status == InvoiceStatus.Paid       ? "#16A34A" :
                                                 d.Status == InvoiceStatus.Cancelled  ? "#DC2626" : "#D97706";
                            col.Item().PaddingTop(6)
                               .Text(statusTxt).Bold().FontSize(14).FontColor(statusColor);
                        });
                    });

                    // ── CONTENT ─────────────────────────────────────────────────
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(16);

                        // Firma & Müşteri
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(fc =>
                            {
                                fc.Item().Text("FİRMA BİLGİLERİ").SemiBold().FontColor(Colors.Grey.Darken2);
                                fc.Item().PaddingTop(4).Text("CarFleet Pro").Bold();
                                fc.Item().Text("Atakum, Samsun, Türkiye");
                                fc.Item().Text("info@carfleetpro.com");
                                fc.Item().Text("0850 123 45 67");
                            });

                            row.ConstantItem(30);

                            row.RelativeItem().Column(cc =>
                            {
                                cc.Item().Text("MÜŞTERİ BİLGİLERİ").SemiBold().FontColor(Colors.Grey.Darken2);
                                cc.Item().PaddingTop(4).Text(d.CustomerName).Bold();
                                cc.Item().Text(d.CustomerAddress);
                                cc.Item().Text(d.CustomerEmail);
                                cc.Item().Text(d.CustomerPhone);
                            });
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Hizmet tablosu
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(28);  // #
                                cols.RelativeColumn(4);   // Açıklama
                                cols.RelativeColumn(1);   // Süre
                                cols.RelativeColumn(1);   // Günlük
                                cols.RelativeColumn(1);   // Toplam
                            });

                            // Başlık
                            static IContainer TH(IContainer c) =>
                                c.Background("#1D4ED8").Padding(7);

                            table.Header(h =>
                            {
                                h.Cell().Element(TH).Text("#").FontColor(Colors.White).Bold();
                                h.Cell().Element(TH).Text("Açıklama").FontColor(Colors.White).Bold();
                                h.Cell().Element(TH).AlignRight().Text("Süre").FontColor(Colors.White).Bold();
                                h.Cell().Element(TH).AlignRight().Text("Günlük").FontColor(Colors.White).Bold();
                                h.Cell().Element(TH).AlignRight().Text("Tutar").FontColor(Colors.White).Bold();
                            });

                            // Veri satırı
                            static IContainer TD(IContainer c) =>
                                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(7);

                            table.Cell().Element(TD).Text("1");
                            table.Cell().Element(TD)
                                 .Text($"{d.VehicleBrand} {d.VehicleModel} ({d.VehiclePlate})\n" +
                                       $"{d.StartDate:dd.MM.yyyy} – {d.EndDate:dd.MM.yyyy}");
                            table.Cell().Element(TD).AlignRight().Text($"{days} gün");
                            table.Cell().Element(TD).AlignRight().Text($"{d.DailyRate:N2} TL");
                            table.Cell().Element(TD).AlignRight().Text($"{d.Amount:N2} TL");
                        });

                        // Özet
                        col.Item().AlignRight().Column(sc =>
                        {
                            sc.Spacing(5);
                            sc.Item().Text($"Ara Toplam  :  {d.Amount:N2} TL").FontSize(11);
                            sc.Item().Text($"KDV (%18)   :  {kdv:N2} TL").FontSize(11);
                            sc.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            sc.Item().Text($"GENEL TOPLAM  :  {total:N2} TL")
                              .FontSize(14).Bold();
                        });

                        // Notlar
                        if (!string.IsNullOrWhiteSpace(d.Notes))
                        {
                            col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(nc =>
                            {
                                nc.Item().Text("Not:").SemiBold();
                                nc.Item().Text(d.Notes);
                            });
                        }

                        // İmza alanı
                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().Column(sc =>
                            {
                                sc.Item().Text("Yetkili İmzası").FontSize(10).FontColor(Colors.Grey.Medium);
                                sc.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                sc.Item().Text("CarFleet Pro").FontSize(10);
                            });
                            row.ConstantItem(60);
                            row.RelativeItem().Column(sc =>
                            {
                                sc.Item().Text("Müşteri İmzası").FontSize(10).FontColor(Colors.Grey.Medium);
                                sc.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                sc.Item().Text(d.CustomerName).FontSize(10);
                            });
                        });
                    });

                    // ── FOOTER ──────────────────────────────────────────────────
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem()
                           .Text("CarFleet Pro — carfleetpro.com | Bu belge bilgisayar ortamında üretilmiştir.")
                           .FontSize(8).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(60).AlignRight().Text(t =>
                        {
                            t.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                            t.CurrentPageNumber().FontSize(8);
                            t.Span(" / ").FontSize(8);
                            t.TotalPages().FontSize(8);
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
