using CarFleetPro.API.Data;
using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/notification — Giriş yapan kullanıcının bildirimlerini getir.
        /// Hem kendine gönderilen hem de tüm kullanıcılara gönderilen (TargetUserId == null) bildirimler gelir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = await _context.Notifications
                .Where(n => n.TargetUserId == null || n.TargetUserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    SentAt = n.SentAt,
                    RelatedVehicleId = n.RelatedVehicleId,
                    RelatedRentalId = n.RelatedRentalId
                })
                .ToListAsync();

            return Ok(notifications);
        }

        /// <summary>
        /// POST /api/notification/send — Bildirim gönder (Sadece Yönetici).
        /// TargetUserId boş bırakılırsa tüm kullanıcılara gönderilir.
        /// </summary>
        [HttpPost("send")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> Send([FromBody] SendNotificationDto dto)
        {
            if (!Enum.TryParse<NotificationType>(dto.Type, out var notifType))
                return BadRequest("Geçersiz bildirim tipi. Geçerli değerler: Push, SMS, Email");

            var notification = new Notification
            {
                Title = dto.Title,
                Message = dto.Message,
                Type = notifType,
                TargetUserId = string.IsNullOrEmpty(dto.TargetUserId) ? null : dto.TargetUserId,
                RelatedVehicleId = dto.RelatedVehicleId,
                RelatedRentalId = dto.RelatedRentalId,
                SentAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var target = string.IsNullOrEmpty(dto.TargetUserId) ? "Tüm kullanıcılar" : "Belirli kullanıcı";
            return Ok(new { message = $"Bildirim gönderildi. Hedef: {target}", notificationId = notification.NotificationId });
        }

        /// <summary>PUT /api/notification/{id}/read — Bildirimi okundu olarak işaretle</summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id &&
                    (n.TargetUserId == null || n.TargetUserId == userId));

            if (notification == null) return NotFound("Bildirim bulunamadı.");

            _context.Attach(notification);
            notification.IsRead = true;
            _context.Entry(notification).Property(n => n.IsRead).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }
    }
}
