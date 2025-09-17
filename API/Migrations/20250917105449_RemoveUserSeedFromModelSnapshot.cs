using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserSeedFromModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordBackdoor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BillingType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false),
                    Salt = table.Column<string>(type: "text", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordBackdoor = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoomTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_RoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    RoomTypeId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordBackdoor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_RoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingServices",
                columns: table => new
                {
                    BookingsId = table.Column<string>(type: "text", nullable: false),
                    ServicesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingServices", x => new { x.BookingsId, x.ServicesId });
                    table.ForeignKey(
                        name: "FK_BookingServices_Bookings_BookingsId",
                        column: x => x.BookingsId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingServices_Services_ServicesId",
                        column: x => x.ServicesId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "PasswordBackdoor", "UpdatedAt" },
                values: new object[,]
                {
                    { "1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard bruger", "User", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rengøringspersonale", "Housekeeping", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "3", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Receptionspersonale", "Receptionist", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "4", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hotel Manager", "Manager", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "5", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Administrator", "Admin", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "RoomTypes",
                columns: new[] { "Id", "BasePrice", "Capacity", "Description", "Name" },
                values: new object[,]
                {
                    { 1, 800m, 1, "Hyggeligt enkeltværelse med alt hvad du behøver.", "Single Room" },
                    { 2, 1200m, 2, "Rummeligt dobbeltværelse med plads til to.", "Double Room" },
                    { 3, 2500m, 4, "Luksuriøs suite med separat opholdsområde og fantastisk udsigt.", "Suite" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "BillingType", "Category", "Description", "IsActive", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 3, "Mad & Drikke", "Lækker morgenmad serveret direkte på dit værelse.", true, "Morgenmad på værelset", 150.00m },
                    { 2, 0, "Mad & Drikke", "En afkølet flaske Moët & Chandon venter på værelset.", true, "Champagne ved ankomst", 400.00m },
                    { 3, 0, "Mad & Drikke", "Et udvalg af sæsonens friske, eksotiske frugter.", true, "Luksus Frugtkurv", 120.00m },
                    { 4, 0, "Mad & Drikke", "Håndlavet luksuschokolade og franske macarons.", true, "Chokolade & Macarons", 95.00m },
                    { 5, 0, "Mad & Drikke", "Premium Gin og tonic-vand med garniture. Skab din egen perfekte drink.", true, "Gin & Tonic Kit", 180.00m },
                    { 6, 2, "Mad & Drikke", "Udsøgt sushi fra hotellets restaurant, leveret til dit værelse.", true, "Sushi Menu", 350.00m },
                    { 7, 0, "Mad & Drikke", "En kurateret smagsoplevelse med udsøgte vine og oste i hotellets vinkælder.", true, "Vin & Ostesmagning", 500.00m },
                    { 8, 0, "Mad & Drikke", "Et udvalg af salte og søde snacks, perfekte til en filmaften på værelset.", true, "Late Night Snacks", 85.00m },
                    { 9, 2, "Wellness & Afslapning", "Fuld dagsadgang til vores luksuriøse spa- og wellnessområde.", true, "Spa Adgang", 250.00m },
                    { 10, 0, "Wellness & Afslapning", "Afslappende massage for to i vores private suite.", true, "60 min. Par-massage", 1200.00m },
                    { 11, 0, "Wellness & Afslapning", "Få en yogamatte og en guide til morgen-yoga på værelset.", true, "Yogamatte og instruktion", 50.00m },
                    { 12, 0, "Wellness & Afslapning", "En privat træningssession med en certificeret træner i hotellets fitnesscenter.", true, "Personlig Træner", 450.00m },
                    { 13, 0, "Wellness & Afslapning", "Book vores private sauna til en times eksklusiv brug.", true, "Privat Sauna Session", 300.00m },
                    { 14, 2, "Wellness & Afslapning", "Hjemmesko og en luksuriøs badekåbe til at tage med hjem.", true, "Badekåbe & Sutsko", 180.00m },
                    { 15, 0, "Praktisk & Komfort", "Sov lidt længere og nyd værelset i et par ekstra timer.", true, "Sen Udtjekning (kl. 14:00)", 200.00m },
                    { 16, 1, "Praktisk & Komfort", "Garanteret parkeringsplads i vores overvågede kælder.", true, "Sikker Parkering", 150.00m },
                    { 17, 0, "Praktisk & Komfort", "Privat luksusbil til eller fra lufthavnen.", true, "Lufthavnstransfer", 600.00m },
                    { 18, 0, "Praktisk & Komfort", "Ekstra grundig rengøring af dit værelse under opholdet.", true, "Ekstra Rengøring", 250.00m },
                    { 19, 0, "Praktisk & Komfort", "Få dit tøj vasket, tørret og strøget inden kl. 18:00.", true, "Tøjvask & Strygning", 120.00m },
                    { 20, 0, "Praktisk & Komfort", "Få minibaren fyldt med dine favorit-drikke og snacks.", true, "Minibar Refill", 0.00m },
                    { 21, 0, "Særlige Lejligheder", "Rosenblade på sengen, stearinlys og en flaske Cava.", true, "Romantisk Pakke", 450.00m },
                    { 22, 0, "Særlige Lejligheder", "En smuk, frisk buket fra vores lokale florist på værelset.", true, "Friske Blomster", 250.00m },
                    { 23, 0, "Særlige Lejligheder", "En lækker, speciallavet kage til at fejre den store dag.", true, "Fødselsdagskage", 300.00m },
                    { 24, 1, "Særlige Lejligheder", "En privat butler er tilgængelig for at imødekomme alle dine behov.", true, "Butler Service", 2000.00m }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "RoomNumber", "RoomTypeId", "Status" },
                values: new object[,]
                {
                    { 1, "101", 1, "Clean" },
                    { 2, "102", 1, "Clean" },
                    { 3, "103", 1, "Clean" },
                    { 4, "104", 1, "Clean" },
                    { 5, "105", 1, "Clean" },
                    { 6, "106", 1, "Clean" },
                    { 7, "107", 1, "Clean" },
                    { 8, "108", 1, "Clean" },
                    { 9, "109", 1, "Clean" },
                    { 10, "110", 1, "Clean" },
                    { 11, "111", 1, "Clean" },
                    { 12, "112", 1, "Clean" },
                    { 13, "113", 1, "Clean" },
                    { 14, "114", 1, "Clean" },
                    { 15, "115", 1, "Clean" },
                    { 16, "116", 1, "Clean" },
                    { 17, "117", 1, "Clean" },
                    { 18, "118", 1, "Clean" },
                    { 19, "119", 1, "Clean" },
                    { 20, "120", 1, "Clean" },
                    { 21, "201", 2, "Clean" },
                    { 22, "202", 2, "Clean" },
                    { 23, "203", 2, "Clean" },
                    { 24, "204", 2, "Clean" },
                    { 25, "205", 2, "Clean" },
                    { 26, "206", 2, "Clean" },
                    { 27, "207", 2, "Clean" },
                    { 28, "208", 2, "Clean" },
                    { 29, "209", 2, "Clean" },
                    { 30, "210", 2, "Clean" },
                    { 31, "211", 2, "Clean" },
                    { 32, "212", 2, "Clean" },
                    { 33, "213", 2, "Clean" },
                    { 34, "214", 2, "Clean" },
                    { 35, "215", 2, "Clean" },
                    { 36, "216", 2, "Clean" },
                    { 37, "217", 2, "Clean" },
                    { 38, "218", 2, "Clean" },
                    { 39, "219", 2, "Clean" },
                    { 40, "220", 2, "Clean" },
                    { 41, "301", 3, "Clean" },
                    { 42, "302", 3, "Clean" },
                    { 43, "303", 3, "Clean" },
                    { 44, "304", 3, "Clean" },
                    { 45, "305", 3, "Clean" },
                    { 46, "306", 3, "Clean" },
                    { 47, "307", 3, "Clean" },
                    { 48, "308", 3, "Clean" },
                    { 49, "309", 3, "Clean" },
                    { 50, "310", 3, "Clean" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomTypeId",
                table: "Bookings",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServices_ServicesId",
                table: "BookingServices",
                column: "ServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomNumber",
                table: "Rooms",
                column: "RoomNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomTypeId",
                table: "Rooms",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingServices");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "RoomTypes");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
