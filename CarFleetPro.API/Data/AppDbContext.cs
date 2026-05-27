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

        public DbSet<VehicleImage> VehicleImages { get; set; } = null!;

        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<DamageRecord> DamageRecords { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        public DbSet<CarBrand> CarBrands { get; set; } = null!;
        public DbSet<CarModel> CarModels { get; set; } = null!;
        public DbSet<CarColor> CarColors { get; set; } = null!;
        public DbSet<PricePolicy> PricePolicies { get; set; } = null!;
        public DbSet<CarType> CarTypes { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber)
                .IsUnique();

            builder.Entity<Customer>()
                .HasIndex(c => c.IdentityNumber)
                .IsUnique();

            builder.Entity<Vehicle>()
                .HasOne(v => v.Brand)
                .WithMany()
                .HasForeignKey(v => v.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany()
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Color)
                .WithMany()
                .HasForeignKey(v => v.ColorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Type)
                .WithMany()
                .HasForeignKey(v => v.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VehicleImage>()
                .HasOne(vi => vi.Vehicle)
                .WithMany()
                .HasForeignKey(vi => vi.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VehicleImage>()
                .HasOne(vi => vi.UploadedByUser)
                .WithMany()
                .HasForeignKey(vi => vi.UploadedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<DamageRecord>()
                .HasOne(d => d.Vehicle)
                .WithMany()
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Maintenance>()
                .HasOne(m => m.Vehicle)
                .WithMany()
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Invoice>()
                .HasOne(i => i.Rental)
                .WithMany()
                .HasForeignKey(i => i.RentalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey(n => n.TargetUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}