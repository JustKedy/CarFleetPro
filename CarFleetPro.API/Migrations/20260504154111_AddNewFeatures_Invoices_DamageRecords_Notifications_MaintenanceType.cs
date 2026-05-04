using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarFleetPro.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFeatures_Invoices_DamageRecords_Notifications_MaintenanceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Branch kolonu zaten olabilir, IF NOT EXISTS ile ekle
            migrationBuilder.Sql("ALTER TABLE \"Vehicles\" ADD COLUMN IF NOT EXISTS \"Branch\" text NOT NULL DEFAULT 'Merkez Şube';");

            // Bakım tipi ve sonraki muayene tarihi (yeni kolonlar)
            migrationBuilder.Sql("ALTER TABLE \"Maintenances\" ADD COLUMN IF NOT EXISTS \"MaintenanceType\" integer NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE \"Maintenances\" ADD COLUMN IF NOT EXISTS \"NextInspectionDate\" timestamp with time zone;");

            // CarBrands tablosu — zaten olabilir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CarBrands"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Name"" text NOT NULL
                );");

            // CarColors tablosu — zaten olabilir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CarColors"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Name"" text NOT NULL
                );");

            // CarModels tablosu — zaten olabilir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CarModels"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""BrandId"" integer NOT NULL,
                    ""Name"" text NOT NULL
                );");

            // DamageRecords tablosu — yeni
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DamageRecords"" (
                    ""DamageId"" SERIAL PRIMARY KEY,
                    ""VehicleId"" integer NOT NULL,
                    ""DamageType"" text NOT NULL DEFAULT '',
                    ""Date"" timestamp with time zone NOT NULL,
                    ""EstimatedCost"" numeric NOT NULL,
                    ""Status"" integer NOT NULL DEFAULT 0,
                    ""PhotoUrl"" text,
                    ""Notes"" text,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""FK_DamageRecords_Vehicles_VehicleId"" FOREIGN KEY (""VehicleId"")
                        REFERENCES ""Vehicles"" (""VehicleId"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_DamageRecords_VehicleId"" ON ""DamageRecords"" (""VehicleId"");");

            // Invoices tablosu — yeni
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Invoices"" (
                    ""InvoiceId"" SERIAL PRIMARY KEY,
                    ""RentalId"" integer NOT NULL,
                    ""TahsilatTarihi"" timestamp with time zone NOT NULL,
                    ""Tutar"" numeric NOT NULL,
                    ""Status"" integer NOT NULL DEFAULT 0,
                    ""OdemYontemi"" integer NOT NULL DEFAULT 0,
                    ""Notes"" text,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""FK_Invoices_Rentals_RentalId"" FOREIGN KEY (""RentalId"")
                        REFERENCES ""Rentals"" (""RentalId"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Invoices_RentalId"" ON ""Invoices"" (""RentalId"");");

            // Notifications tablosu — yeni
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Notifications"" (
                    ""NotificationId"" SERIAL PRIMARY KEY,
                    ""TargetUserId"" text,
                    ""Type"" integer NOT NULL DEFAULT 0,
                    ""Title"" text NOT NULL DEFAULT '',
                    ""Message"" text NOT NULL DEFAULT '',
                    ""IsRead"" boolean NOT NULL DEFAULT false,
                    ""SentAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""FK_Notifications_AspNetUsers_TargetUserId"" FOREIGN KEY (""TargetUserId"")
                        REFERENCES ""AspNetUsers"" (""Id"") ON DELETE SET NULL
                );");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Notifications_TargetUserId"" ON ""Notifications"" (""TargetUserId"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CarBrands");
            migrationBuilder.DropTable(name: "CarColors");
            migrationBuilder.DropTable(name: "CarModels");
            migrationBuilder.DropTable(name: "DamageRecords");
            migrationBuilder.DropTable(name: "Invoices");
            migrationBuilder.DropTable(name: "Notifications");

            migrationBuilder.DropColumn(name: "Branch", table: "Vehicles");
            migrationBuilder.DropColumn(name: "MaintenanceType", table: "Maintenances");
            migrationBuilder.DropColumn(name: "NextInspectionDate", table: "Maintenances");
        }
    }
}
