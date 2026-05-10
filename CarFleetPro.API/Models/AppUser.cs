using Microsoft.AspNetCore.Identity;

namespace CarFleetPro.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // "Yönetici" veya "Çalışan" — Identity role sistemiyle senkronize tutulur
        public string Role { get; set; } = "Çalışan";

        public bool IsActive { get; set; } = true;

        // Çalışanın departmanı (isteğe bağlı)
        public string? Department { get; set; }

        // Hesap oluşturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Bildirim tercihleri
        public bool MaintenanceAlerts { get; set; } = true;
        public bool RentalExpiryAlerts { get; set; } = true;
        public bool InstantAvailabilityAlerts { get; set; } = true;
    }
}
