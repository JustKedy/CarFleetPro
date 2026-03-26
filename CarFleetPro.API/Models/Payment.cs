namespace CarFleetPro.API.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int RentalId { get; set; }
        public Rental? Rental { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsPaid { get; set; } = false;
    }
}
