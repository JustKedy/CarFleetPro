using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarFleetPro.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Veri bütünlüğü için test verilerini (araçları) ve bağlı oldukları kayıtları uçuruyoruz
            migrationBuilder.Sql("DELETE FROM \"VehicleImages\";");
            migrationBuilder.Sql("DELETE FROM \"Rentals\";");
            migrationBuilder.Sql("DELETE FROM \"DamageRecords\";");
            migrationBuilder.Sql("DELETE FROM \"Maintenances\";");
            migrationBuilder.Sql("DELETE FROM \"Vehicles\";");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Segment",
                table: "Vehicles");

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);



            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_BrandId",
                table: "Vehicles",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ColorId",
                table: "Vehicles",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ModelId",
                table: "Vehicles",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TypeId",
                table: "Vehicles",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_CarBrands_BrandId",
                table: "Vehicles",
                column: "BrandId",
                principalTable: "CarBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_CarColors_ColorId",
                table: "Vehicles",
                column: "ColorId",
                principalTable: "CarColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_CarModels_ModelId",
                table: "Vehicles",
                column: "ModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_CarTypes_TypeId",
                table: "Vehicles",
                column: "TypeId",
                principalTable: "CarTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_CarBrands_BrandId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_CarColors_ColorId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_CarModels_ModelId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_CarTypes_TypeId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "CarTypes");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_BrandId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_ColorId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_ModelId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TypeId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Vehicles");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Vehicles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                table: "Vehicles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
