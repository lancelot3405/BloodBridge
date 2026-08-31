using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodBridge.API.Migrations
{
    /// <inheritdoc />
    public partial class LinkBloodRequestsToRequesters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequesterId",
                table: "BloodRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BloodRequests",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequesterId",
                value: null);

            migrationBuilder.UpdateData(
                table: "BloodRequests",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequesterId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequesterId",
                table: "BloodRequests");
        }
    }
}
