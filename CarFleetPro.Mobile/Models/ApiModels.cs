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
        public decimal MonthlyRevenue { get; set; }
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
}
