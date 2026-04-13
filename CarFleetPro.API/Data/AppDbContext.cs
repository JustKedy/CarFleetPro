using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Data
{
    
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
            
            
            ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
        }

        
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;

        
        public DbSet<CarBrand> CarBrands { get; set; } = null!;
        public DbSet<CarModel> CarModels { get; set; } = null!;
        public DbSet<CarColor> CarColors { get; set; } = null!;

        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber)
                .IsUnique();

            
            builder.Entity<Customer>()
                .HasIndex(c => c.IdentityNumber)
                .IsUnique();
        }
    }
}