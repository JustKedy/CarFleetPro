namespace CarFleetPro.Mobile.Models
{
    public class CustomerInfo
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool HasActiveRental { get; set; }
        public string RentalStatus { get; set; } = string.Empty;
        public int TotalRentals { get; set; }
    }

    public class CustomerName
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AlertInfo
    {
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string AlertColor { get; set; } = "#EF4444";
        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
    }

    public class UserProfile
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool MaintenanceAlerts { get; set; }
        public bool RentalExpiryAlerts { get; set; }
        public bool InstantAvailabilityAlerts { get; set; }
    }

    public class VehicleDetail
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string TransmissionType { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public int HorsePower { get; set; }
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public string Branch { get; set; } = string.Empty;
        public List<HistoryItem> History { get; set; } = new();
    }

    public class HistoryItem
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Amount { get; set; }
        public string Color { get; set; } = "#3B82F6";
    }

    public class DashboardStats
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int RentedVehicles { get; set; }
        public int MaintenanceVehicles { get; set; }
        public decimal? MonthlyRevenue { get; set; }
        public double RentedPercentage { get; set; }
        public double AvailablePercentage { get; set; }
        public double MaintenancePercentage { get; set; }
        public List<TopModel> TopModels { get; set; } = new();
    }

    public class TopModel
    {
        public string ModelName { get; set; } = string.Empty;
        public int RentCount { get; set; }
    }

    public class VehicleImageInfo
    {
        public int VehicleImageId { get; set; }
        public int VehicleId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class RentalInfo
    {
        public int RentalId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MaintenanceInfo
    {
        public int MaintenanceId { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMaintenanceRequest
    {
        public int VehicleId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal Cost { get; set; }
    }

    public class UpdateMaintenanceRequest
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal? Cost { get; set; }
    }

    public class DamageInfo
    {
        public int DamageRecordId { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string DamageType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DamageDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDamageRecordRequest
    {
        public int VehicleId { get; set; }
        public string DamageType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DamageDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class UpdateDamageRecordRequest
    {
        public string Status { get; set; } = string.Empty;
        public decimal? EstimatedCost { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class InvoiceInfo
    {
        public int InvoiceId { get; set; }
        public int RentalId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateInvoiceRequest
    {
        public int RentalId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateInvoiceRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class NotificationInfo
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

    public class SendNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? TargetUserId { get; set; }
        public int? RelatedVehicleId { get; set; }
        public int? RelatedRentalId { get; set; }
    }

    public class StaffInfo
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class CustomerDetail : CustomerInfo
    {
        public string IdentityNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public DateTime DriverLicenseExpiry { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsBlacklisted { get; set; }
        public List<RentalHistoryItem> RentalHistory { get; set; } = new();
    }

    public class RentalHistoryItem
    {
        public int RentalId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateCustomerRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public DateTime DriverLicenseExpiry { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
