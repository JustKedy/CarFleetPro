namespace CarFleetPro.API.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int RentedVehicles { get; set; }
        public int VehiclesInMaintenance { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; } // Kasaya giren toplam para
    }
}