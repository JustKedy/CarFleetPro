using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Data
{
    // IdentityDbContext'e kendi oluşturduğumuz AppUser sınıfını veriyoruz
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak tablolarımız
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;

        // 🚗 Araç Marka, Model ve Renk (Lookup) Tabloları
        public DbSet<CarBrand> CarBrands { get; set; } = null!;
        public DbSet<CarModel> CarModels { get; set; } = null!;
        public DbSet<CarColor> CarColors { get; set; } = null!;

        // (Kiralama, Ödeme ve Bakım tablolarını yazınca buraya ekleyeceğiz)

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Planındaki kurala göre Plaka benzersiz (Unique) olmalı
            builder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber)
                .IsUnique();

            // Müşterinin TC Kimlik numarası benzersiz (Unique) olmalı
            builder.Entity<Customer>()
                .HasIndex(c => c.IdentityNumber)
                .IsUnique();
        }
    }
}