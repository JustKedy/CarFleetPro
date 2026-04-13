using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarFleetPro.API.Migrations
{
    
    public partial class AddNotificationSettings : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HorsePower",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InstantAvailabilityAlerts",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MaintenanceAlerts",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RentalExpiryAlerts",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HorsePower",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "InstantAvailabilityAlerts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MaintenanceAlerts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RentalExpiryAlerts",
                table: "AspNetUsers");
        }
    }
}
