namespace CarFleetPro.API.DTOs
{
    public class CreateCustomerDto
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

    
    
    
    public class CustomerListDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty; 
        public string PhoneNumber { get; set; } = string.Empty;
        public bool HasActiveRental { get; set; }
        public string RentalStatus { get; set; } = string.Empty; 
        public int TotalRentals { get; set; }
    }

    
    
    
    public class CustomerDetailDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public DateTime DriverLicenseExpiry { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsBlacklisted { get; set; }
        public List<RentalHistoryDto> RentalHistory { get; set; } = new();
    }

    public class RentalHistoryDto
    {
        public int RentalId { get; set; }
        public string VehicleName { get; set; } = string.Empty; 
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty; 
    }
}