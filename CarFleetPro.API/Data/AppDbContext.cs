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

        // Ana tablolar
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;

        // Araç fotoğrafları (çoklu)
        public DbSet<VehicleImage> VehicleImages { get; set; } = null!;

        // Yeni tablolar
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<DamageRecord> DamageRecords { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Lookup tabloları
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

            // VehicleImage → Vehicle
            builder.Entity<VehicleImage>()
                .HasOne(vi => vi.Vehicle)
                .WithMany()
                .HasForeignKey(vi => vi.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // VehicleImage → AppUser
            builder.Entity<VehicleImage>()
                .HasOne(vi => vi.UploadedByUser)
                .WithMany()
                .HasForeignKey(vi => vi.UploadedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // DamageRecord → Vehicle (silme engellemek için Restrict)
            builder.Entity<DamageRecord>()
                .HasOne(d => d.Vehicle)
                .WithMany()
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice → Rental
            builder.Entity<Invoice>()
                .HasOne(i => i.Rental)
                .WithMany()
                .HasForeignKey(i => i.RentalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification → AppUser (null olabilir: tüm kullanıcı bildirimi)
            builder.Entity<Notification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey(n => n.TargetUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}