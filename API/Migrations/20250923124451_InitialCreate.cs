using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LongDescription = table.Column<string>(type: "text", nullable: false),
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
                name: "RoomTypeServices",
                columns: table => new
                {
                    RoomTypesId = table.Column<int>(type: "integer", nullable: false),
                    ServicesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypeServices", x => new { x.RoomTypesId, x.ServicesId });
                    table.ForeignKey(
                        name: "FK_RoomTypeServices_RoomTypes_RoomTypesId",
                        column: x => x.RoomTypesId,
                        principalTable: "RoomTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomTypeServices_Services_ServicesId",
                        column: x => x.ServicesId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    GuestName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GuestEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedToUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordBackdoor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TicketId = table.Column<string>(type: "text", nullable: false),
                    SenderId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsInternalNote = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordBackdoor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Users_SenderId",
                        column: x => x.SenderId,
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
                columns: new[] { "Id", "BasePrice", "Capacity", "LongDescription", "Name", "ShortDescription" },
                values: new object[,]
                {
                    { 1, 800m, 2, "Vores Standard Værelse byder på 25 velindrettede kvadratmeter med en luksuriøs queen-size seng, skrivebord og et moderne badeværelse med regnbruser. Nyd faciliteter som high-speed WiFi, et 4K Smart TV med streaming, og en minibar. Værelset er designet med rene linjer og en rolig farvepalette for at sikre et afslappende ophold.", "Standard Værelse", "Et elegant og komfortabelt værelse, perfekt til forretningsrejsen eller en weekendtur." },
                    { 2, 2200m, 4, "Vores Deluxe Suite på 55 kvadratmeter er indbegrebet af moderne luksus. Suiten har et separat soveværelse med en king-size seng og en rummelig opholdsstue med sofaarrangement og spiseplads. Fra de store panoramavinduer har du en fantastisk udsigt over byen. Badeværelset er udstyret med både badekar og en separat regnbruser, samt eksklusive toiletartikler.", "Deluxe Suite", "Oplev ekstra plads og luksus med en separat opholdsstue og en imponerende udsigt." },
                    { 3, 5000m, 8, "Presidential Suiten er mere end et værelse; det er indbegrebet af kompromisløs luksus og Flyhigh Hotels mest prestigefyldte residens. Træd ind i en verden af raffineret elegance, hvor 120 kvadratmeter er dedikeret til din absolutte komfort.\r\n\r\nDen ekspansive opholdsstue er et statement i sig selv med nøje udvalgt kunst, designer-møbler og et imponerende flygel. Gulv-til-loft-vinduer bader rummet i naturligt lys.\r\n\r\nSuiten råder over to separate soveværelser, hver med luksuriøse king-size senge og tilhørende spa-lignende marmorbadeværelser. Det fuldt udstyrede gourmetkøkken giver desuden mulighed for, at vores private kok kan kreere skræddersyede kulinariske oplevelser direkte i suiten.\r\n", "Presidential Suite", "Den ultimative luksusoplevelse fordelt på 120 kvadratmeter med eksklusiv service." }
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
                    { 5, 0, "Mad & Drikke", "Premium Gin og tonic-vand med garniture.", true, "Gin & Tonic Kit", 180.00m },
                    { 6, 2, "Mad & Drikke", "Udsøgt sushi fra hotellets restaurant, leveret til dit værelse.", true, "Sushi Menu", 350.00m },
                    { 7, 0, "Mad & Drikke", "En kurateret smagsoplevelse med udsøgte vine og oste.", true, "Vin & Ostesmagning", 500.00m },
                    { 8, 0, "Mad & Drikke", "Et udvalg af salte og søde snacks.", true, "Late Night Snacks", 85.00m },
                    { 9, 2, "Wellness & Afslapning", "Fuld dagsadgang til vores luksuriøse spa- og wellnessområde.", true, "Spa Adgang", 250.00m },
                    { 10, 0, "Wellness & Afslapning", "Afslappende massage for to i vores private suite.", true, "60 min. Par-massage", 1200.00m },
                    { 11, 0, "Wellness & Afslapning", "Få en yogamåtte og en guide til morgen-yoga.", true, "Yogamåtte og instruktion", 50.00m },
                    { 12, 0, "Wellness & Afslapning", "En privat træningssession med en certificeret træner.", true, "Personlig Træner", 450.00m },
                    { 13, 0, "Wellness & Afslapning", "Book vores private sauna til en times eksklusiv brug.", true, "Privat Sauna Session", 300.00m },
                    { 14, 2, "Wellness & Afslapning", "Hjemmesko og en luksuriøs badekåbe til at tage med hjem.", true, "Badekåbe & Sutsko", 180.00m },
                    { 15, 0, "Praktisk & Komfort", "Sov lidt længere og nyd værelset i et par ekstra timer.", true, "Sen Udtjekning (kl. 14:00)", 200.00m },
                    { 16, 1, "Praktisk & Komfort", "Garanteret parkeringsplads i vores overvågede kælder.", true, "Sikker Parkering", 150.00m },
                    { 17, 0, "Praktisk & Komfort", "Privat luksusbil til eller fra lufthavnen.", true, "Lufthavnstransfer", 600.00m },
                    { 18, 0, "Praktisk & Komfort", "Ekstra grundig rengøring af dit værelse under opholdet.", true, "Ekstra Rengøring", 250.00m },
                    { 19, 0, "Praktisk & Komfort", "Få dit tøj vasket, tørret og strøget.", true, "Tøjvask & Strygning", 120.00m },
                    { 20, 0, "Praktisk & Komfort", "Få minibaren fyldt med dine favorit-drikke og snacks.", true, "Minibar Refill", 0.00m },
                    { 21, 0, "Særlige Lejligheder", "Rosenblade på sengen, stearinlys og en flaske Cava.", true, "Romantisk Pakke", 450.00m },
                    { 22, 0, "Særlige Lejligheder", "En smuk, frisk buket fra vores lokale florist.", true, "Friske Blomster", 250.00m },
                    { 23, 0, "Særlige Lejligheder", "En lækker, speciallavet kage til at fejre den store dag.", true, "Fødselsdagskage", 300.00m },
                    { 24, 1, "Særlige Lejligheder", "En privat butler er tilgængelig 24/7.", true, "Butler Service", 2000.00m }
                });

            migrationBuilder.InsertData(
                table: "RoomTypeServices",
                columns: new[] { "RoomTypesId", "ServicesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 8 },
                    { 1, 9 },
                    { 1, 11 },
                    { 1, 15 },
                    { 1, 16 },
                    { 1, 19 },
                    { 1, 20 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 2, 5 },
                    { 2, 8 },
                    { 2, 9 },
                    { 2, 11 },
                    { 2, 13 },
                    { 2, 14 },
                    { 2, 15 },
                    { 2, 16 },
                    { 2, 17 },
                    { 2, 18 },
                    { 2, 19 },
                    { 2, 20 },
                    { 2, 22 },
                    { 3, 1 },
                    { 3, 2 },
                    { 3, 3 },
                    { 3, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 3, 7 },
                    { 3, 8 },
                    { 3, 9 },
                    { 3, 10 },
                    { 3, 11 },
                    { 3, 12 },
                    { 3, 13 },
                    { 3, 14 },
                    { 3, 15 },
                    { 3, 16 },
                    { 3, 17 },
                    { 3, 18 },
                    { 3, 19 },
                    { 3, 20 },
                    { 3, 21 },
                    { 3, 22 },
                    { 3, 23 },
                    { 3, 24 }
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
                    { 21, "121", 1, "Clean" },
                    { 22, "122", 1, "Clean" },
                    { 23, "123", 1, "Clean" },
                    { 24, "124", 1, "Clean" },
                    { 25, "125", 1, "Clean" },
                    { 26, "126", 1, "Clean" },
                    { 27, "127", 1, "Clean" },
                    { 28, "128", 1, "Clean" },
                    { 29, "129", 1, "Clean" },
                    { 30, "130", 1, "Clean" },
                    { 31, "131", 1, "Clean" },
                    { 32, "132", 1, "Clean" },
                    { 33, "133", 1, "Clean" },
                    { 34, "134", 1, "Clean" },
                    { 35, "135", 1, "Clean" },
                    { 36, "136", 1, "Clean" },
                    { 37, "137", 1, "Clean" },
                    { 38, "138", 1, "Clean" },
                    { 39, "139", 1, "Clean" },
                    { 40, "140", 1, "Clean" },
                    { 41, "141", 1, "Clean" },
                    { 42, "142", 1, "Clean" },
                    { 43, "143", 1, "Clean" },
                    { 44, "144", 1, "Clean" },
                    { 45, "145", 1, "Clean" },
                    { 46, "146", 1, "Clean" },
                    { 47, "147", 1, "Clean" },
                    { 48, "148", 1, "Clean" },
                    { 49, "149", 1, "Clean" },
                    { 50, "150", 1, "Clean" },
                    { 51, "151", 1, "Clean" },
                    { 52, "152", 1, "Clean" },
                    { 53, "153", 1, "Clean" },
                    { 54, "154", 1, "Clean" },
                    { 55, "155", 1, "Clean" },
                    { 56, "156", 1, "Clean" },
                    { 57, "157", 1, "Clean" },
                    { 58, "158", 1, "Clean" },
                    { 59, "159", 1, "Clean" },
                    { 60, "160", 1, "Clean" },
                    { 61, "161", 1, "Clean" },
                    { 62, "162", 1, "Clean" },
                    { 63, "163", 1, "Clean" },
                    { 64, "164", 1, "Clean" },
                    { 65, "165", 1, "Clean" },
                    { 66, "166", 1, "Clean" },
                    { 67, "167", 1, "Clean" },
                    { 68, "168", 1, "Clean" },
                    { 69, "169", 1, "Clean" },
                    { 70, "170", 1, "Clean" },
                    { 71, "171", 1, "Clean" },
                    { 72, "172", 1, "Clean" },
                    { 73, "173", 1, "Clean" },
                    { 74, "174", 1, "Clean" },
                    { 75, "175", 1, "Clean" },
                    { 76, "176", 1, "Clean" },
                    { 77, "177", 1, "Clean" },
                    { 78, "178", 1, "Clean" },
                    { 79, "179", 1, "Clean" },
                    { 80, "180", 1, "Clean" },
                    { 81, "181", 1, "Clean" },
                    { 82, "182", 1, "Clean" },
                    { 83, "183", 1, "Clean" },
                    { 84, "184", 1, "Clean" },
                    { 85, "185", 1, "Clean" },
                    { 86, "186", 1, "Clean" },
                    { 87, "187", 1, "Clean" },
                    { 88, "188", 1, "Clean" },
                    { 89, "189", 1, "Clean" },
                    { 90, "190", 1, "Clean" },
                    { 91, "191", 1, "Clean" },
                    { 92, "192", 1, "Clean" },
                    { 93, "193", 1, "Clean" },
                    { 94, "194", 1, "Clean" },
                    { 95, "195", 1, "Clean" },
                    { 96, "196", 1, "Clean" },
                    { 97, "197", 1, "Clean" },
                    { 98, "198", 1, "Clean" },
                    { 99, "199", 1, "Clean" },
                    { 100, "200", 1, "Clean" },
                    { 101, "201", 1, "Clean" },
                    { 102, "202", 1, "Clean" },
                    { 103, "203", 1, "Clean" },
                    { 104, "204", 1, "Clean" },
                    { 105, "205", 1, "Clean" },
                    { 106, "206", 1, "Clean" },
                    { 107, "207", 1, "Clean" },
                    { 108, "208", 1, "Clean" },
                    { 109, "209", 1, "Clean" },
                    { 110, "210", 1, "Clean" },
                    { 111, "211", 1, "Clean" },
                    { 112, "212", 1, "Clean" },
                    { 113, "213", 1, "Clean" },
                    { 114, "214", 1, "Clean" },
                    { 115, "215", 1, "Clean" },
                    { 116, "216", 1, "Clean" },
                    { 117, "217", 1, "Clean" },
                    { 118, "218", 1, "Clean" },
                    { 119, "219", 1, "Clean" },
                    { 120, "220", 1, "Clean" },
                    { 121, "221", 1, "Clean" },
                    { 122, "222", 1, "Clean" },
                    { 123, "223", 1, "Clean" },
                    { 124, "224", 1, "Clean" },
                    { 125, "225", 1, "Clean" },
                    { 126, "226", 1, "Clean" },
                    { 127, "227", 1, "Clean" },
                    { 128, "228", 1, "Clean" },
                    { 129, "229", 1, "Clean" },
                    { 130, "230", 1, "Clean" },
                    { 131, "231", 1, "Clean" },
                    { 132, "232", 1, "Clean" },
                    { 133, "233", 1, "Clean" },
                    { 134, "234", 1, "Clean" },
                    { 135, "235", 1, "Clean" },
                    { 136, "236", 1, "Clean" },
                    { 137, "237", 1, "Clean" },
                    { 138, "238", 1, "Clean" },
                    { 139, "239", 1, "Clean" },
                    { 140, "240", 1, "Clean" },
                    { 141, "241", 1, "Clean" },
                    { 142, "242", 1, "Clean" },
                    { 143, "243", 1, "Clean" },
                    { 144, "244", 1, "Clean" },
                    { 145, "245", 1, "Clean" },
                    { 146, "246", 1, "Clean" },
                    { 147, "247", 1, "Clean" },
                    { 148, "248", 1, "Clean" },
                    { 149, "249", 1, "Clean" },
                    { 150, "250", 1, "Clean" },
                    { 151, "251", 1, "Clean" },
                    { 152, "252", 1, "Clean" },
                    { 153, "253", 1, "Clean" },
                    { 154, "254", 1, "Clean" },
                    { 155, "255", 1, "Clean" },
                    { 156, "256", 1, "Clean" },
                    { 157, "257", 1, "Clean" },
                    { 158, "258", 1, "Clean" },
                    { 159, "259", 1, "Clean" },
                    { 160, "260", 1, "Clean" },
                    { 161, "261", 1, "Clean" },
                    { 162, "262", 1, "Clean" },
                    { 163, "263", 1, "Clean" },
                    { 164, "264", 1, "Clean" },
                    { 165, "265", 1, "Clean" },
                    { 166, "266", 1, "Clean" },
                    { 167, "267", 1, "Clean" },
                    { 168, "268", 1, "Clean" },
                    { 169, "269", 1, "Clean" },
                    { 170, "270", 1, "Clean" },
                    { 171, "271", 1, "Clean" },
                    { 172, "272", 1, "Clean" },
                    { 173, "273", 1, "Clean" },
                    { 174, "274", 1, "Clean" },
                    { 175, "275", 1, "Clean" },
                    { 176, "276", 1, "Clean" },
                    { 177, "277", 1, "Clean" },
                    { 178, "278", 1, "Clean" },
                    { 179, "279", 1, "Clean" },
                    { 180, "280", 1, "Clean" },
                    { 181, "281", 1, "Clean" },
                    { 182, "282", 1, "Clean" },
                    { 183, "283", 1, "Clean" },
                    { 184, "284", 1, "Clean" },
                    { 185, "285", 1, "Clean" },
                    { 186, "286", 1, "Clean" },
                    { 187, "287", 1, "Clean" },
                    { 188, "288", 1, "Clean" },
                    { 189, "289", 1, "Clean" },
                    { 190, "290", 1, "Clean" },
                    { 191, "291", 1, "Clean" },
                    { 192, "292", 1, "Clean" },
                    { 193, "293", 1, "Clean" },
                    { 194, "294", 1, "Clean" },
                    { 195, "295", 1, "Clean" },
                    { 196, "296", 1, "Clean" },
                    { 197, "297", 1, "Clean" },
                    { 198, "298", 1, "Clean" },
                    { 199, "299", 1, "Clean" },
                    { 200, "300", 1, "Clean" },
                    { 201, "301", 1, "Clean" },
                    { 202, "302", 1, "Clean" },
                    { 203, "303", 1, "Clean" },
                    { 204, "304", 1, "Clean" },
                    { 205, "305", 1, "Clean" },
                    { 206, "306", 1, "Clean" },
                    { 207, "307", 1, "Clean" },
                    { 208, "308", 1, "Clean" },
                    { 209, "309", 1, "Clean" },
                    { 210, "310", 1, "Clean" },
                    { 211, "311", 1, "Clean" },
                    { 212, "312", 1, "Clean" },
                    { 213, "313", 1, "Clean" },
                    { 214, "314", 1, "Clean" },
                    { 215, "315", 1, "Clean" },
                    { 216, "316", 1, "Clean" },
                    { 217, "317", 1, "Clean" },
                    { 218, "318", 1, "Clean" },
                    { 219, "319", 1, "Clean" },
                    { 220, "320", 1, "Clean" },
                    { 221, "321", 1, "Clean" },
                    { 222, "322", 1, "Clean" },
                    { 223, "323", 1, "Clean" },
                    { 224, "324", 1, "Clean" },
                    { 225, "325", 1, "Clean" },
                    { 226, "326", 1, "Clean" },
                    { 227, "327", 1, "Clean" },
                    { 228, "328", 1, "Clean" },
                    { 229, "329", 1, "Clean" },
                    { 230, "330", 1, "Clean" },
                    { 231, "331", 1, "Clean" },
                    { 232, "332", 1, "Clean" },
                    { 233, "333", 1, "Clean" },
                    { 234, "334", 1, "Clean" },
                    { 235, "335", 1, "Clean" },
                    { 236, "336", 1, "Clean" },
                    { 237, "337", 1, "Clean" },
                    { 238, "338", 1, "Clean" },
                    { 239, "339", 1, "Clean" },
                    { 240, "340", 1, "Clean" },
                    { 241, "341", 1, "Clean" },
                    { 242, "342", 1, "Clean" },
                    { 243, "343", 1, "Clean" },
                    { 244, "344", 1, "Clean" },
                    { 245, "345", 1, "Clean" },
                    { 246, "346", 1, "Clean" },
                    { 247, "347", 1, "Clean" },
                    { 248, "348", 1, "Clean" },
                    { 249, "349", 1, "Clean" },
                    { 250, "350", 1, "Clean" },
                    { 251, "351", 2, "Clean" },
                    { 252, "352", 2, "Clean" },
                    { 253, "353", 2, "Clean" },
                    { 254, "354", 2, "Clean" },
                    { 255, "355", 2, "Clean" },
                    { 256, "356", 2, "Clean" },
                    { 257, "357", 2, "Clean" },
                    { 258, "358", 2, "Clean" },
                    { 259, "359", 2, "Clean" },
                    { 260, "360", 2, "Clean" },
                    { 261, "361", 2, "Clean" },
                    { 262, "362", 2, "Clean" },
                    { 263, "363", 2, "Clean" },
                    { 264, "364", 2, "Clean" },
                    { 265, "365", 2, "Clean" },
                    { 266, "366", 2, "Clean" },
                    { 267, "367", 2, "Clean" },
                    { 268, "368", 2, "Clean" },
                    { 269, "369", 2, "Clean" },
                    { 270, "370", 2, "Clean" },
                    { 271, "371", 2, "Clean" },
                    { 272, "372", 2, "Clean" },
                    { 273, "373", 2, "Clean" },
                    { 274, "374", 2, "Clean" },
                    { 275, "375", 2, "Clean" },
                    { 276, "376", 2, "Clean" },
                    { 277, "377", 2, "Clean" },
                    { 278, "378", 2, "Clean" },
                    { 279, "379", 2, "Clean" },
                    { 280, "380", 2, "Clean" },
                    { 281, "381", 2, "Clean" },
                    { 282, "382", 2, "Clean" },
                    { 283, "383", 2, "Clean" },
                    { 284, "384", 2, "Clean" },
                    { 285, "385", 2, "Clean" },
                    { 286, "386", 2, "Clean" },
                    { 287, "387", 2, "Clean" },
                    { 288, "388", 2, "Clean" },
                    { 289, "389", 2, "Clean" },
                    { 290, "390", 2, "Clean" },
                    { 291, "391", 2, "Clean" },
                    { 292, "392", 2, "Clean" },
                    { 293, "393", 2, "Clean" },
                    { 294, "394", 2, "Clean" },
                    { 295, "395", 2, "Clean" },
                    { 296, "396", 2, "Clean" },
                    { 297, "397", 2, "Clean" },
                    { 298, "398", 2, "Clean" },
                    { 299, "399", 2, "Clean" },
                    { 300, "400", 2, "Clean" },
                    { 301, "401", 2, "Clean" },
                    { 302, "402", 2, "Clean" },
                    { 303, "403", 2, "Clean" },
                    { 304, "404", 2, "Clean" },
                    { 305, "405", 2, "Clean" },
                    { 306, "406", 2, "Clean" },
                    { 307, "407", 2, "Clean" },
                    { 308, "408", 2, "Clean" },
                    { 309, "409", 2, "Clean" },
                    { 310, "410", 2, "Clean" },
                    { 311, "411", 2, "Clean" },
                    { 312, "412", 2, "Clean" },
                    { 313, "413", 2, "Clean" },
                    { 314, "414", 2, "Clean" },
                    { 315, "415", 2, "Clean" },
                    { 316, "416", 2, "Clean" },
                    { 317, "417", 2, "Clean" },
                    { 318, "418", 2, "Clean" },
                    { 319, "419", 2, "Clean" },
                    { 320, "420", 2, "Clean" },
                    { 321, "421", 2, "Clean" },
                    { 322, "422", 2, "Clean" },
                    { 323, "423", 2, "Clean" },
                    { 324, "424", 2, "Clean" },
                    { 325, "425", 2, "Clean" },
                    { 326, "426", 2, "Clean" },
                    { 327, "427", 2, "Clean" },
                    { 328, "428", 2, "Clean" },
                    { 329, "429", 2, "Clean" },
                    { 330, "430", 2, "Clean" },
                    { 331, "431", 2, "Clean" },
                    { 332, "432", 2, "Clean" },
                    { 333, "433", 2, "Clean" },
                    { 334, "434", 2, "Clean" },
                    { 335, "435", 2, "Clean" },
                    { 336, "436", 2, "Clean" },
                    { 337, "437", 2, "Clean" },
                    { 338, "438", 2, "Clean" },
                    { 339, "439", 2, "Clean" },
                    { 340, "440", 2, "Clean" },
                    { 341, "441", 2, "Clean" },
                    { 342, "442", 2, "Clean" },
                    { 343, "443", 2, "Clean" },
                    { 344, "444", 2, "Clean" },
                    { 345, "445", 2, "Clean" },
                    { 346, "446", 2, "Clean" },
                    { 347, "447", 2, "Clean" },
                    { 348, "448", 2, "Clean" },
                    { 349, "449", 2, "Clean" },
                    { 350, "450", 2, "Clean" },
                    { 351, "451", 3, "Clean" },
                    { 352, "452", 3, "Clean" },
                    { 353, "453", 3, "Clean" },
                    { 354, "454", 3, "Clean" },
                    { 355, "455", 3, "Clean" },
                    { 356, "456", 3, "Clean" },
                    { 357, "457", 3, "Clean" },
                    { 358, "458", 3, "Clean" },
                    { 359, "459", 3, "Clean" },
                    { 360, "460", 3, "Clean" },
                    { 361, "461", 3, "Clean" },
                    { 362, "462", 3, "Clean" },
                    { 363, "463", 3, "Clean" },
                    { 364, "464", 3, "Clean" },
                    { 365, "465", 3, "Clean" },
                    { 366, "466", 3, "Clean" },
                    { 367, "467", 3, "Clean" },
                    { 368, "468", 3, "Clean" },
                    { 369, "469", 3, "Clean" },
                    { 370, "470", 3, "Clean" },
                    { 371, "471", 3, "Clean" },
                    { 372, "472", 3, "Clean" },
                    { 373, "473", 3, "Clean" },
                    { 374, "474", 3, "Clean" },
                    { 375, "475", 3, "Clean" },
                    { 376, "476", 3, "Clean" },
                    { 377, "477", 3, "Clean" },
                    { 378, "478", 3, "Clean" },
                    { 379, "479", 3, "Clean" },
                    { 380, "480", 3, "Clean" },
                    { 381, "481", 3, "Clean" },
                    { 382, "482", 3, "Clean" },
                    { 383, "483", 3, "Clean" },
                    { 384, "484", 3, "Clean" },
                    { 385, "485", 3, "Clean" },
                    { 386, "486", 3, "Clean" },
                    { 387, "487", 3, "Clean" },
                    { 388, "488", 3, "Clean" },
                    { 389, "489", 3, "Clean" },
                    { 390, "490", 3, "Clean" },
                    { 391, "491", 3, "Clean" },
                    { 392, "492", 3, "Clean" },
                    { 393, "493", 3, "Clean" },
                    { 394, "494", 3, "Clean" },
                    { 395, "495", 3, "Clean" },
                    { 396, "496", 3, "Clean" },
                    { 397, "497", 3, "Clean" },
                    { 398, "498", 3, "Clean" },
                    { 399, "499", 3, "Clean" },
                    { 400, "500", 3, "Clean" }
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
                name: "IX_MeetingRoomBookings_MeetingRoomId",
                table: "MeetingRoomBookings",
                column: "MeetingRoomId");

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
                name: "IX_RoomTypeServices_ServicesId",
                table: "RoomTypeServices",
                column: "ServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_SenderId",
                table: "TicketMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedToUserId",
                table: "Tickets",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedByUserId",
                table: "Tickets",
                column: "CreatedByUserId");

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
                name: "MeetingRoomBookings");

            migrationBuilder.DropTable(
                name: "RoomTypeServices");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "MeetingRooms");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Tickets");

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
