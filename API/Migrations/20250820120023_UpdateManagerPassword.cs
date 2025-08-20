using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManagerPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "f2b72c57-632b-4a88-a476-2a1c72787e9c",
                column: "HashedPassword",
                value: "$2a$11$e.h9qi30632t342.k2R9P.3hF3sA7rq1qV48z4cAM3q2y2j5n5q6m");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "f2b72c57-632b-4a88-a476-2a1c72787e9c",
                column: "HashedPassword",
                value: "$2a$11$68i5aSoi96exP02R/Ljo..KOQJ0JW/m1aXb3dKGNfUu0w9dG3Cz/e");
        }
    }
}
