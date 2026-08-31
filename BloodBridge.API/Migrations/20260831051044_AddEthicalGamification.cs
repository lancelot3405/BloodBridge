using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodBridge.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEthicalGamification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GamificationActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonorId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActivityKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BloodRequestId = table.Column<int>(type: "int", nullable: true),
                    PointsAwarded = table.Column<int>(type: "int", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamificationActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamificationActivities_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GamificationProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonorId = table.Column<int>(type: "int", nullable: false),
                    ImpactScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TierRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "New Donor"),
                    BadgesEarned = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileCompletedXPGranted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamificationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamificationProfiles_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamificationActivities_DonorId_ActivityKey",
                table: "GamificationActivities",
                columns: new[] { "DonorId", "ActivityKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GamificationProfiles_DonorId",
                table: "GamificationProfiles",
                column: "DonorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamificationActivities");

            migrationBuilder.DropTable(
                name: "GamificationProfiles");
        }
    }
}
