namespace CarFleetPro.API.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty; // TC Kimlik (Benzersiz olacak)
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string DriverLicenseNumber { get; set; } = string.Empty; // Ehliyet No
        public DateTime DriverLicenseExpiry { get; set; } // Ehliyet Geçerlilik
        public string Address { get; set; } = string.Empty;
        public bool IsBlacklisted { get; set; } = false; // Kara liste
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
