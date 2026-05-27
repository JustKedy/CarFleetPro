using Microsoft.AspNetCore.Identity;

namespace CarFleetPro.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "Çalışan";

        public bool IsActive { get; set; } = true;

        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool MaintenanceAlerts { get; set; } = true;
        public bool RentalExpiryAlerts { get; set; } = true;
        public bool InstantAvailabilityAlerts { get; set; } = true;
    }
}
