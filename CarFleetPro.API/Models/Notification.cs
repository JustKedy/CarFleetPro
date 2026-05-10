namespace CarFleetPro.API.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        // Gönderilen kullanıcı (null = tüm kullanıcılar)
        public string? TargetUserId { get; set; }
        public AppUser? TargetUser { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Push;
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // İlgili araç/kiralama (opsiyonel)
        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
    }
}
