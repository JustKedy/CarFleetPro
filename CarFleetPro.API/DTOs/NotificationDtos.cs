namespace CarFleetPro.API.DTOs
{
    public class SendNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>Push, SMS, Email</summary>
        public string Type { get; set; } = "Push";

        /// <summary>Null = tüm kullanıcılar, dolu = belirli kullanıcı</summary>
        public string? TargetUserId { get; set; }

        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
    }
}
