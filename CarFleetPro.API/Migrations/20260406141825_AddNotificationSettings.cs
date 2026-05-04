using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarFleetPro.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bu kolonlar manuel SQL ile zaten eklenmiş olabilir, IF NOT EXISTS ile güvenli ekle
            migrationBuilder.Sql("ALTER TABLE \"Vehicles\" ADD COLUMN IF NOT EXISTS \"Color\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Vehicles\" ADD COLUMN IF NOT EXISTS \"HorsePower\" integer NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE \"Vehicles\" ADD COLUMN IF NOT EXISTS \"ImageUrl\" text;");
            migrationBuilder.Sql("ALTER TABLE \"Vehicles\" ADD COLUMN IF NOT EXISTS \"Branch\" text NOT NULL DEFAULT 'Merkez Şube';");

            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ADD COLUMN IF NOT EXISTS \"InstantAvailabilityAlerts\" boolean NOT NULL DEFAULT true;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ADD COLUMN IF NOT EXISTS \"MaintenanceAlerts\" boolean NOT NULL DEFAULT true;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ADD COLUMN IF NOT EXISTS \"RentalExpiryAlerts\" boolean NOT NULL DEFAULT true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Color", table: "Vehicles");
            migrationBuilder.DropColumn(name: "HorsePower", table: "Vehicles");
            migrationBuilder.DropColumn(name: "ImageUrl", table: "Vehicles");
            migrationBuilder.DropColumn(name: "Branch", table: "Vehicles");
            migrationBuilder.DropColumn(name: "InstantAvailabilityAlerts", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MaintenanceAlerts", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "RentalExpiryAlerts", table: "AspNetUsers");
        }
    }
}
