namespace CarFleetPro.API.Models
{
    public class CarBrand { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    public class CarModel { public int Id { get; set; } public int BrandId { get; set; } public string Name { get; set; } = string.Empty; }
    public class CarColor { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
}