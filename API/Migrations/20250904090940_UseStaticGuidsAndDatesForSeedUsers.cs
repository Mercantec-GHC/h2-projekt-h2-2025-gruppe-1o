using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UseStaticGuidsAndDatesForSeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "32b729e4-1bab-4b8e-ada0-bf94b1d44b89");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "7060d476-83bc-4dfa-8882-e86298a69cdc");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "efdc99eb-c414-4f6d-b589-1bc80ac947d5");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "HashedPassword", "LastLogin", "LastName", "PasswordBackdoor", "PhoneNumber", "RoleId", "Salt", "UpdatedAt" },
                values: new object[,]
                {
                    { "a1b38c7f-9a2d-4e8f-8f3a-3c1b7e5d2a9f", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "receptionist@hotel.dk", "Receptionist", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Test", "Password123!", "", "3", null, new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { "c5d8a9b2-3e4f-4a1b-9d8c-7b6a5d4c3b2a", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "rengøring@hotel.dk", "Rengøring", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Test", "Password123!", "", "2", null, new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { "f2b72c57-632b-4a88-a476-2a1c72787e9c", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "manager@hotel.dk", "Manager", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Admin", "Password123!", "", "4", null, new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "a1b38c7f-9a2d-4e8f-8f3a-3c1b7e5d2a9f");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "c5d8a9b2-3e4f-4a1b-9d8c-7b6a5d4c3b2a");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "f2b72c57-632b-4a88-a476-2a1c72787e9c");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "HashedPassword", "LastLogin", "LastName", "PasswordBackdoor", "PhoneNumber", "RoleId", "Salt", "UpdatedAt" },
                values: new object[,]
                {
                    { "32b729e4-1bab-4b8e-ada0-bf94b1d44b89", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "rengøring@hotel.dk", "Rengøring", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "Test", "Password123!", "", "2", null, new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486) },
                    { "7060d476-83bc-4dfa-8882-e86298a69cdc", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "receptionist@hotel.dk", "Receptionist", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "Test", "Password123!", "", "3", null, new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486) },
                    { "efdc99eb-c414-4f6d-b589-1bc80ac947d5", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "manager@hotel.dk", "Manager", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "Admin", "Password123!", "", "4", null, new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486) }
                });
        }
    }
}
