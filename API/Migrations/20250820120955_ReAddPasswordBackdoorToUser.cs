using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ReAddPasswordBackdoorToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordBackdoor",
                table: "Roles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordBackdoor",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "1",
                column: "PasswordBackdoor",
                value: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "2",
                column: "PasswordBackdoor",
                value: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "3",
                column: "PasswordBackdoor",
                value: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "4",
                column: "PasswordBackdoor",
                value: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "f2b72c57-632b-4a88-a476-2a1c72787e9c",
                column: "PasswordBackdoor",
                value: "Password123!");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordBackdoor",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "PasswordBackdoor",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "f2b72c57-632b-4a88-a476-2a1c72787e9c",
                column: "PasswordBackdoor",
                value: "");
        }
    }
}
