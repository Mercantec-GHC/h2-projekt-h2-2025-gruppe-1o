using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomAndBookingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RoomNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PricePerNight = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoomId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "CreatedAt", "Description", "PricePerNight", "RoomNumber", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { "24492a37-7a9a-4b5a-a8b0-f19fce657397", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4763), "Luksuriøs suite med separat stue og havudsigt.", 2500m, "301", "Suite", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4764) },
                    { "4c8deffd-d269-4ceb-9148-49ac966598cc", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4759), "Rummeligt dobbeltværelse med balkon.", 1200m, "201", "Double", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4760) },
                    { "8cddeb60-5944-4fc2-a720-e14441c036f6", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4733), "Hyggeligt enkeltværelse med udsigt over gården.", 800m, "101", "Single", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4734) },
                    { "91aa791f-39b5-4128-bd03-e5a78592fcc0", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4757), "Hyggeligt enkeltværelse med udsigt over gården.", 800m, "102", "Single", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4757) },
                    { "bc04bf05-9bad-4619-b3c3-4d5171b0fd48", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4761), "Rummeligt dobbeltværelse med balkon.", 1200m, "202", "Double", new DateTime(2025, 8, 14, 8, 12, 29, 472, DateTimeKind.Utc).AddTicks(4762) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomNumber",
                table: "Rooms",
                column: "RoomNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
