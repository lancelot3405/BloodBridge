using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodBridge.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImpactHistoryStreaksAndSeasonalLeaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStreak",
                table: "GamificationProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "GamificationProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActiveDate",
                table: "GamificationProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ImpactActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonorId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpactActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImpactActivityLogs_Donors_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImpactActivityLogs_DonorId_EarnedAt",
                table: "ImpactActivityLogs",
                columns: new[] { "DonorId", "EarnedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImpactActivityLogs");

            migrationBuilder.DropColumn(
                name: "CurrentStreak",
                table: "GamificationProfiles");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "GamificationProfiles");

            migrationBuilder.DropColumn(
                name: "LastActiveDate",
                table: "GamificationProfiles");
        }
    }
}
