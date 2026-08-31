using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodBridge.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestWorkflowAndDonationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Donations_BloodRequestId",
                table: "Donations");

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Donations",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE donation
                SET BloodGroup = request.BloodGroup
                FROM Donations AS donation
                INNER JOIN BloodRequests AS request
                    ON request.Id = donation.BloodRequestId;
                """);

            migrationBuilder.AddColumn<int>(
                name: "AcceptedDonorId",
                table: "BloodRequests",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BloodRequests",
                keyColumn: "Id",
                keyValue: 1,
                column: "AcceptedDonorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "BloodRequests",
                keyColumn: "Id",
                keyValue: 2,
                column: "AcceptedDonorId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_BloodRequestId_DonorId",
                table: "Donations",
                columns: new[] { "BloodRequestId", "DonorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_AcceptedDonorId",
                table: "BloodRequests",
                column: "AcceptedDonorId");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodRequests_Donors_AcceptedDonorId",
                table: "BloodRequests",
                column: "AcceptedDonorId",
                principalTable: "Donors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodRequests_Donors_AcceptedDonorId",
                table: "BloodRequests");

            migrationBuilder.DropIndex(
                name: "IX_Donations_BloodRequestId_DonorId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_BloodRequests_AcceptedDonorId",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "AcceptedDonorId",
                table: "BloodRequests");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_BloodRequestId",
                table: "Donations",
                column: "BloodRequestId");
        }
    }
}
