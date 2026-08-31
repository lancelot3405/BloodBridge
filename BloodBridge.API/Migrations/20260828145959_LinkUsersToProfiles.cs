using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BloodBridge.API.Migrations
{
    /// <inheritdoc />
    public partial class LinkUsersToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Hospitals",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Donors",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Requesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requesters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requesters_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "00000000-0000-0000-0000-000000000001", 0, "00000000-0000-0000-0000-000000000001", "seed-donor-1@bloodbridge.local", true, false, null, "SEED-DONOR-1@BLOODBRIDGE.LOCAL", "SEED-DONOR-1@BLOODBRIDGE.LOCAL", null, null, false, "00000000-0000-0000-0000-000000000001", false, "seed-donor-1@bloodbridge.local" },
                    { "00000000-0000-0000-0000-000000000002", 0, "00000000-0000-0000-0000-000000000002", "seed-donor-2@bloodbridge.local", true, false, null, "SEED-DONOR-2@BLOODBRIDGE.LOCAL", "SEED-DONOR-2@BLOODBRIDGE.LOCAL", null, null, false, "00000000-0000-0000-0000-000000000002", false, "seed-donor-2@bloodbridge.local" },
                    { "00000000-0000-0000-0000-000000000003", 0, "00000000-0000-0000-0000-000000000003", "seed-donor-3@bloodbridge.local", true, false, null, "SEED-DONOR-3@BLOODBRIDGE.LOCAL", "SEED-DONOR-3@BLOODBRIDGE.LOCAL", null, null, false, "00000000-0000-0000-0000-000000000003", false, "seed-donor-3@bloodbridge.local" },
                    { "00000000-0000-0000-0000-000000000011", 0, "00000000-0000-0000-0000-000000000011", "seed-hospital-1@bloodbridge.local", true, false, null, "SEED-HOSPITAL-1@BLOODBRIDGE.LOCAL", "SEED-HOSPITAL-1@BLOODBRIDGE.LOCAL", null, null, false, "00000000-0000-0000-0000-000000000011", false, "seed-hospital-1@bloodbridge.local" },
                    { "00000000-0000-0000-0000-000000000012", 0, "00000000-0000-0000-0000-000000000012", "seed-hospital-2@bloodbridge.local", true, false, null, "SEED-HOSPITAL-2@BLOODBRIDGE.LOCAL", "SEED-HOSPITAL-2@BLOODBRIDGE.LOCAL", null, null, false, "00000000-0000-0000-0000-000000000012", false, "seed-hospital-2@bloodbridge.local" }
                });

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 2,
                column: "UserId",
                value: "00000000-0000-0000-0000-000000000002");

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 3,
                column: "UserId",
                value: "00000000-0000-0000-0000-000000000003");

            migrationBuilder.UpdateData(
                table: "Hospitals",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: "00000000-0000-0000-0000-000000000011");

            migrationBuilder.UpdateData(
                table: "Hospitals",
                keyColumn: "Id",
                keyValue: 2,
                column: "UserId",
                value: "00000000-0000-0000-0000-000000000012");

            migrationBuilder.CreateIndex(
                name: "IX_Hospitals_UserId",
                table: "Hospitals",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donors_UserId",
                table: "Donors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requesters_UserId",
                table: "Requesters",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Donors_AspNetUsers_UserId",
                table: "Donors",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hospitals_AspNetUsers_UserId",
                table: "Hospitals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donors_AspNetUsers_UserId",
                table: "Donors");

            migrationBuilder.DropForeignKey(
                name: "FK_Hospitals_AspNetUsers_UserId",
                table: "Hospitals");

            migrationBuilder.DropTable(
                name: "Requesters");

            migrationBuilder.DropIndex(
                name: "IX_Hospitals_UserId",
                table: "Hospitals");

            migrationBuilder.DropIndex(
                name: "IX_Donors_UserId",
                table: "Donors");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000002");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000003");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000011");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000012");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Hospitals");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Donors",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 2,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Donors",
                keyColumn: "Id",
                keyValue: 3,
                column: "UserId",
                value: null);
        }
    }
}
