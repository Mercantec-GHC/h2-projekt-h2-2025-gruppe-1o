using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingRoomBookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeetingRoomBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeetingRoomId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BookedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BookedByEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRoomBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingRoomBookings_MeetingRooms_MeetingRoomId",
                        column: x => x.MeetingRoomId,
                        principalTable: "MeetingRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MeetingRooms",
                columns: new[] { "Id", "Capacity", "Description", "HourlyRate", "ImageUrl", "Name" },
                values: new object[,]
                {
                    { 1, 12, "Intimt og professionelt lokale med videokonferenceudstyr.", 750m, "/images/meeting-boardroom.jpg", "Bestyrelseslokalet" },
                    { 2, 100, "Stor sal perfekt til præsentationer og større arrangementer.", 2500m, "/images/meeting-conference.jpg", "Konferencesalen" },
                    { 3, 6, "Lille og lyst rum til kreative workshops eller gruppearbejde.", 400m, "/images/meeting-breakout.jpg", "Grupperum Alfa" },
                    { 4, 8, "Fleksibelt rum med whiteboard og plads til samarbejde.", 500m, "/images/meeting-breakout-2.jpg", "Grupperum Beta" },
                    { 5, 50, "Moderne auditorium med biografopstilling og AV-udstyr i topklasse.", 1800m, "/images/meeting-auditorium.jpg", "Auditoriet" }
                });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Få en yogamåtte og en guide til morgen-yoga.", "Yogamåtte og instruktion" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRoomBookings_MeetingRoomId",
                table: "MeetingRoomBookings",
                column: "MeetingRoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingRoomBookings");

            migrationBuilder.DropTable(
                name: "MeetingRooms");

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Få en yogamatte og en guide til morgen-yoga.", "Yogamatte og instruktion" });
        }
    }
}
