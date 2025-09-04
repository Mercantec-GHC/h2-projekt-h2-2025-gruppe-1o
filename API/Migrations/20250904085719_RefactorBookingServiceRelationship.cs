using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBookingServiceRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Bookings_BookingId",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Services_ServiceId",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PasswordBackdoor",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "BookingServices",
                newName: "ServicesId");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "BookingServices",
                newName: "BookingsId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingServices_ServiceId",
                table: "BookingServices",
                newName: "IX_BookingServices_ServicesId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BillingType",
                table: "Services",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "HashedPassword", "LastLogin", "LastName", "PasswordBackdoor", "PhoneNumber", "RoleId", "Salt", "UpdatedAt" },
                values: new object[,]
                {
                    { "32b729e4-1bab-4b8e-ada0-bf94b1d44b89", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "rengøring@hotel.dk", "Rengøring", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "Test", "Password123!", "", "2", null, new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486) },
                    { "7060d476-83bc-4dfa-8882-e86298a69cdc", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "receptionist@hotel.dk", "Receptionist", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "Test", "Password123!", "", "3", null, new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486) },
                    { "efdc99eb-c414-4f6d-b589-1bc80ac947d5", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "manager@hotel.dk", "Manager", "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W", new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486), "Admin", "Password123!", "", "4", null, new DateTime(2025, 9, 4, 8, 57, 19, 493, DateTimeKind.Utc).AddTicks(2486) }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Bookings_BookingsId",
                table: "BookingServices",
                column: "BookingsId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Services_ServicesId",
                table: "BookingServices",
                column: "ServicesId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Bookings_BookingsId",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Services_ServicesId",
                table: "BookingServices");

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

            migrationBuilder.RenameColumn(
                name: "ServicesId",
                table: "BookingServices",
                newName: "ServiceId");

            migrationBuilder.RenameColumn(
                name: "BookingsId",
                table: "BookingServices",
                newName: "BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingServices_ServicesId",
                table: "BookingServices",
                newName: "IX_BookingServices_ServiceId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "BillingType",
                table: "Services",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Services",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PasswordBackdoor",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Services",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordBackdoor", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordBackdoor", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordBackdoor", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Bookings_BookingId",
                table: "BookingServices",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Services_ServiceId",
                table: "BookingServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
