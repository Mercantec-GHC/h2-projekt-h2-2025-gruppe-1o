using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedServiceAndBookingLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Capacity", "Name" },
                values: new object[] { 2, "Standard Værelse" });

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BasePrice", "Capacity", "Description", "Name" },
                values: new object[] { 2200m, 4, "Rummelig suite med panoramaudsigt og premium faciliteter.", "Deluxe Suite" });

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BasePrice", "Capacity", "Description", "Name" },
                values: new object[] { 5000m, 8, "Den ultimative luksusoplevelse med eksklusiv service.", "Presidential Suite" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Capacity", "Name" },
                values: new object[] { 1, "Single Room" });

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BasePrice", "Capacity", "Description", "Name" },
                values: new object[] { 1200m, 2, "Rummeligt dobbeltværelse med plads til to.", "Double Room" });

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BasePrice", "Capacity", "Description", "Name" },
                values: new object[] { 2500m, 4, "Luksuriøs suite med separat opholdsområde og fantastisk udsigt.", "Suite" });
        }
    }
}
