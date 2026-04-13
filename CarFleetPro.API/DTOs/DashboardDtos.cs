namespace CarFleetPro.API.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int RentedVehicles { get; set; }
        public decimal MonthlyRevenue { get; set; } 

        
        public double RentedPercentage { get; set; }
        public double AvailablePercentage { get; set; }
        public double MaintenancePercentage { get; set; }

        
        public List<TopModelDto> TopModels { get; set; } = new();
    }

    public class TopModelDto
    {
        public string ModelName { get; set; } = string.Empty;
        public int RentCount { get; set; } 
    }
}