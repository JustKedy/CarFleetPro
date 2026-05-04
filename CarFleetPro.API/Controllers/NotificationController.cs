using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    // Madde 7 — Bildirim Yönetimi Controller
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NotificationController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST /api/notification/send — Hedef kitleye bildirim gönder
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendNotificationDto dto)
        {
            var notifications = new List<Notification>();

            if (dto.TargetUserIds != null && dto.TargetUserIds.Any())
            {
                // Belirli kullanıcılara gönder
                foreach (var userId in dto.TargetUserIds)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null || !user.IsActive) continue;

                    notifications.Add(new Notification
                    {
                        TargetUserId = userId,
                        Type = dto.Type,
                        Title = dto.Title,
                        Message = dto.Message,
                        IsRead = false,
                        SentAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                // Tüm aktif kullanıcılara broadcast
                var allUsers = await _userManager.Users
                    .Where(u => u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var userId in allUsers)
                {
                    notifications.Add(new Notification
                    {
                        TargetUserId = userId,
                        Type = dto.Type,
                        Title = dto.Title,
                        Message = dto.Message,
                        IsRead = false,
                        SentAt = DateTime.UtcNow
                    });
                }
            }

            if (!notifications.Any())
                return BadRequest("Geçerli alıcı bulunamadı.");

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{notifications.Count} kullanıcıya bildirim gönderildi.",
                sentCount = notifications.Count
            });
        }

        // GET /api/notification — Bildirim geçmişi (opsiyonel: userId filtresi)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? userId)
        {
            var query = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(n => n.TargetUserId == userId);

            var result = await query
                .OrderByDescending(n => n.SentAt)
                .Take(100) // Son 100 bildirim
                .Select(n => new NotificationListDto
                {
                    NotificationId = n.NotificationId,
                    TargetUserId = n.TargetUserId,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    SentAt = n.SentAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /api/notification/my — Giriş yapan kullanıcının bildirimleri
        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _context.Notifications
                .Where(n => n.TargetUserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Take(50)
                .Select(n => new NotificationListDto
                {
                    NotificationId = n.NotificationId,
                    TargetUserId = n.TargetUserId,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    SentAt = n.SentAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // PUT /api/notification/{id}/read — Bildirimi okundu işaretle
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);
            if (notification == null) return NotFound("Bildirim bulunamadı.");

            _context.Attach(notification);
            notification.IsRead = true;
            _context.Entry(notification).Property(n => n.IsRead).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }
    }
}
