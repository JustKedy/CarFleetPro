using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarFleetPro.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPricePolicyAndVehicleSegments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_Vehicles_VehicleId",
                table: "DamageRecords");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Vehicles",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "MaxDiscountPercentage",
                table: "Vehicles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                table: "Vehicles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PricePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    TargetValue = table.Column<string>(type: "text", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxDiscountPercentage = table.Column<double>(type: "double precision", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricePolicies", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_Vehicles_VehicleId",
                table: "DamageRecords",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_Vehicles_VehicleId",
                table: "DamageRecords");

            migrationBuilder.DropTable(
                name: "PricePolicies");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MaxDiscountPercentage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Segment",
                table: "Vehicles");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_Vehicles_VehicleId",
                table: "DamageRecords",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
