namespace CarFleetPro.API.Models
{
    public enum VehicleType { Sedan, SUV, Hatchback, Van }
    public enum FuelType { Benzin, Dizel, Elektrik }
    public enum TransmissionType { Manuel, Otomatik }
    public enum VehicleStatus { Available, Rented, Maintenance }

    public enum RentalStatus { Active, Completed, Cancelled }
    public enum PaymentMethod { Nakit, KrediKarti, Havale }
    public enum MaintenanceStatus { Planned, InProgress, Done }

    // Bakım tipi — Madde 5
    public enum MaintenanceType { Periyodik, Ariza, Kaza, Diger }

    // Hasar durumu — Madde 6
    public enum DamageStatus { IslemBekliyor, Onarimda, Tamamlandi }

    // Fatura durumu — Madde 4
    public enum InvoiceStatus { Bekliyor, Odendi, Iptal }

    // Bildirim tipi — Madde 7
    public enum NotificationType { Genel, Bakim, KiraSonu, Hasar }
}
