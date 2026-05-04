namespace CarFleetPro.API.Models
{
    // Madde 7 — Bildirim Altyapısı
    public class Notification
    {
        public int NotificationId { get; set; }

        // Alıcı — null ise broadcast
        public string? TargetUserId { get; set; }
        public AppUser? TargetUser { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Genel;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
