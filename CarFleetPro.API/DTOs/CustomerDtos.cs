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
}