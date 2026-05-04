namespace CarFleetPro.API.DTOs
{
    // Madde 7 — Bildirim DTO'ları
    public class SendNotificationDto
    {
        // Belirli kullanıcılar — boş bırakılırsa tüm aktif kullanıcılara gönderilir
        public List<string>? TargetUserIds { get; set; }
        public Models.NotificationType Type { get; set; } = Models.NotificationType.Genel;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class NotificationListDto
    {
        public int NotificationId { get; set; }
        public string? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}
