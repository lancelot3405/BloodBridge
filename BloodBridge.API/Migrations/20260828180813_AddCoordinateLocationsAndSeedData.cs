using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodBridge.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinateLocationsAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Hospitals",
                newName: "Location");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 1,
                column: "Location",
                value: "23.2599,77.4126");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 2,
                column: "Location",
                value: "23.2337,77.4340");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 3,
                column: "Location",
                value: "23.2156,77.4321");

            migrationBuilder.UpdateData(
                table: "Hospitals",
                keyColumn: "Id",
                keyValue: 1,
                column: "Location",
                value: "23.2599,77.4126");

            migrationBuilder.UpdateData(
                table: "Hospitals",
                keyColumn: "Id",
                keyValue: 2,
                column: "Location",
                value: "23.1990,77.3770");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Hospitals",
                newName: "Address");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 1,
                column: "Location",
                value: "Bhopal");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 2,
                column: "Location",
                value: "Bhopal");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 3,
                column: "Location",
                value: "Bhopal");

            migrationBuilder.UpdateData(
                table: "Hospitals",
                keyColumn: "Id",
                keyValue: 1,
                column: "Address",
                value: "MP Nagar, Bhopal");

            migrationBuilder.UpdateData(
                table: "Hospitals",
                keyColumn: "Id",
                keyValue: 2,
                column: "Address",
                value: "Arera Colony, Bhopal");
        }
    }
}
