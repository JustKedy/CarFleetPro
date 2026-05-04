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

        // Mevcut tablolar
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;

        // Lookup tablolar
        public DbSet<CarBrand> CarBrands { get; set; } = null!;
        public DbSet<CarModel> CarModels { get; set; } = null!;
        public DbSet<CarColor> CarColors { get; set; } = null!;

        // Yeni tablolar (Madde 4, 6, 7)
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<DamageRecord> DamageRecords { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Vehicle — unique plaka
            builder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber)
                .IsUnique();

            // Customer — unique TC
            builder.Entity<Customer>()
                .HasIndex(c => c.IdentityNumber)
                .IsUnique();

            // Invoice — PK + FK
            builder.Entity<Invoice>()
                .HasKey(i => i.InvoiceId);
            builder.Entity<Invoice>()
                .Property(i => i.InvoiceId)
                .ValueGeneratedOnAdd();
            builder.Entity<Invoice>()
                .HasOne(i => i.Rental)
                .WithMany()
                .HasForeignKey(i => i.RentalId)
                .OnDelete(DeleteBehavior.Cascade);

            // DamageRecord — PK + FK
            builder.Entity<DamageRecord>()
                .HasKey(d => d.DamageId);
            builder.Entity<DamageRecord>()
                .Property(d => d.DamageId)
                .ValueGeneratedOnAdd();
            builder.Entity<DamageRecord>()
                .HasOne(d => d.Vehicle)
                .WithMany()
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification — PK + nullable FK
            builder.Entity<Notification>()
                .HasKey(n => n.NotificationId);
            builder.Entity<Notification>()
                .Property(n => n.NotificationId)
                .ValueGeneratedOnAdd();
            builder.Entity<Notification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey(n => n.TargetUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}