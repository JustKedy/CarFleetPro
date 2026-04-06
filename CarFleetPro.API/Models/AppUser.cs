using Microsoft.AspNetCore.Identity;

namespace CarFleetPro.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Agent"; // Admin veya Agent
        public bool IsActive { get; set; } = true;

        // Bildirim ayarları
        public bool MaintenanceAlerts { get; set; } = true;
        public bool RentalExpiryAlerts { get; set; } = true;
        public bool InstantAvailabilityAlerts { get; set; } = true;
    }
}
