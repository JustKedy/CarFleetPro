namespace CarFleetPro.API.DTOs
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateNotificationsDto
    {
        public bool MaintenanceAlerts { get; set; }
        public bool RentalExpiryAlerts { get; set; }
        public bool InstantAvailabilityAlerts { get; set; }
    }
}