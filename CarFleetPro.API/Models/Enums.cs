namespace CarFleetPro.API.Models
{
   
    public enum VehicleType { Sedan, SUV, Hatchback, Van }
    public enum FuelType { Benzin, Dizel, Elektrik }
    public enum TransmissionType { Manuel, Otomatik }
    public enum VehicleStatus { Available, Rented, Maintenance }

    public enum RentalStatus { Active, Completed, Cancelled }
    public enum PaymentMethod { Nakit, KrediKarti, Havale }
    public enum MaintenanceStatus { Planned, InProgress, Done }

    public class Enums
    {

    }
}
