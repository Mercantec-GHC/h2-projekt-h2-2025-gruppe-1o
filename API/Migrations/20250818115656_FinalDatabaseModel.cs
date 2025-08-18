using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FinalDatabaseModel : Migration
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
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BillingType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
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
                    Email = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
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
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    BookingId = table.Column<string>(type: "text", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingServices", x => new { x.BookingId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_BookingServices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { "1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard bruger", "User", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rengøringspersonale", "Housekeeping", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "3", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Receptionspersonale", "Receptionist", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "4", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hotel Manager", "Manager", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
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
                columns: new[] { "Id", "BillingType", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "PerNight", "Breakfast", 150m },
                    { 2, "PerBooking", "Spa Access", 250m },
                    { 3, "PerBooking", "Champagne on arrival", 400m }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "RoomNumber", "RoomTypeId", "Status" },
                values: new object[,]
                {
                    { 1, "100", 1, "Clean" },
                    { 2, "101", 1, "Clean" },
                    { 3, "102", 1, "Clean" },
                    { 4, "103", 1, "Clean" },
                    { 5, "104", 1, "Clean" },
                    { 6, "105", 1, "Clean" },
                    { 7, "106", 1, "Clean" },
                    { 8, "107", 1, "Clean" },
                    { 9, "108", 1, "Clean" },
                    { 10, "109", 1, "Clean" },
                    { 11, "110", 1, "Clean" },
                    { 12, "111", 1, "Clean" },
                    { 13, "112", 1, "Clean" },
                    { 14, "113", 1, "Clean" },
                    { 15, "114", 1, "Clean" },
                    { 16, "115", 1, "Clean" },
                    { 17, "116", 1, "Clean" },
                    { 18, "117", 1, "Clean" },
                    { 19, "118", 1, "Clean" },
                    { 20, "119", 1, "Clean" },
                    { 21, "120", 1, "Clean" },
                    { 22, "121", 1, "Clean" },
                    { 23, "122", 1, "Clean" },
                    { 24, "123", 1, "Clean" },
                    { 25, "124", 1, "Clean" },
                    { 26, "125", 1, "Clean" },
                    { 27, "126", 1, "Clean" },
                    { 28, "127", 1, "Clean" },
                    { 29, "128", 1, "Clean" },
                    { 30, "129", 1, "Clean" },
                    { 31, "130", 1, "Clean" },
                    { 32, "131", 1, "Clean" },
                    { 33, "132", 1, "Clean" },
                    { 34, "133", 1, "Clean" },
                    { 35, "134", 1, "Clean" },
                    { 36, "135", 1, "Clean" },
                    { 37, "136", 1, "Clean" },
                    { 38, "137", 1, "Clean" },
                    { 39, "138", 1, "Clean" },
                    { 40, "139", 1, "Clean" },
                    { 41, "140", 1, "Clean" },
                    { 42, "141", 1, "Clean" },
                    { 43, "142", 1, "Clean" },
                    { 44, "143", 1, "Clean" },
                    { 45, "144", 1, "Clean" },
                    { 46, "145", 1, "Clean" },
                    { 47, "146", 1, "Clean" },
                    { 48, "147", 1, "Clean" },
                    { 49, "148", 1, "Clean" },
                    { 50, "149", 1, "Clean" },
                    { 51, "150", 1, "Clean" },
                    { 52, "151", 1, "Clean" },
                    { 53, "152", 1, "Clean" },
                    { 54, "153", 1, "Clean" },
                    { 55, "154", 1, "Clean" },
                    { 56, "155", 1, "Clean" },
                    { 57, "156", 1, "Clean" },
                    { 58, "157", 1, "Clean" },
                    { 59, "158", 1, "Clean" },
                    { 60, "159", 1, "Clean" },
                    { 61, "160", 1, "Clean" },
                    { 62, "161", 1, "Clean" },
                    { 63, "162", 1, "Clean" },
                    { 64, "163", 1, "Clean" },
                    { 65, "164", 1, "Clean" },
                    { 66, "165", 1, "Clean" },
                    { 67, "166", 1, "Clean" },
                    { 68, "167", 1, "Clean" },
                    { 69, "168", 1, "Clean" },
                    { 70, "169", 1, "Clean" },
                    { 71, "170", 1, "Clean" },
                    { 72, "171", 1, "Clean" },
                    { 73, "172", 1, "Clean" },
                    { 74, "173", 1, "Clean" },
                    { 75, "174", 1, "Clean" },
                    { 76, "175", 1, "Clean" },
                    { 77, "176", 1, "Clean" },
                    { 78, "177", 1, "Clean" },
                    { 79, "178", 1, "Clean" },
                    { 80, "179", 1, "Clean" },
                    { 81, "180", 1, "Clean" },
                    { 82, "181", 1, "Clean" },
                    { 83, "182", 1, "Clean" },
                    { 84, "183", 1, "Clean" },
                    { 85, "184", 1, "Clean" },
                    { 86, "185", 1, "Clean" },
                    { 87, "186", 1, "Clean" },
                    { 88, "187", 1, "Clean" },
                    { 89, "188", 1, "Clean" },
                    { 90, "189", 1, "Clean" },
                    { 91, "190", 1, "Clean" },
                    { 92, "191", 1, "Clean" },
                    { 93, "192", 1, "Clean" },
                    { 94, "193", 1, "Clean" },
                    { 95, "194", 1, "Clean" },
                    { 96, "195", 1, "Clean" },
                    { 97, "196", 1, "Clean" },
                    { 98, "197", 1, "Clean" },
                    { 99, "198", 1, "Clean" },
                    { 100, "199", 1, "Clean" },
                    { 101, "200", 1, "Clean" },
                    { 102, "201", 1, "Clean" },
                    { 103, "202", 1, "Clean" },
                    { 104, "203", 1, "Clean" },
                    { 105, "204", 1, "Clean" },
                    { 106, "205", 1, "Clean" },
                    { 107, "206", 1, "Clean" },
                    { 108, "207", 1, "Clean" },
                    { 109, "208", 1, "Clean" },
                    { 110, "209", 1, "Clean" },
                    { 111, "210", 1, "Clean" },
                    { 112, "211", 1, "Clean" },
                    { 113, "212", 1, "Clean" },
                    { 114, "213", 1, "Clean" },
                    { 115, "214", 1, "Clean" },
                    { 116, "215", 1, "Clean" },
                    { 117, "216", 1, "Clean" },
                    { 118, "217", 1, "Clean" },
                    { 119, "218", 1, "Clean" },
                    { 120, "219", 1, "Clean" },
                    { 121, "220", 1, "Clean" },
                    { 122, "221", 1, "Clean" },
                    { 123, "222", 1, "Clean" },
                    { 124, "223", 1, "Clean" },
                    { 125, "224", 1, "Clean" },
                    { 126, "225", 1, "Clean" },
                    { 127, "226", 1, "Clean" },
                    { 128, "227", 1, "Clean" },
                    { 129, "228", 1, "Clean" },
                    { 130, "229", 1, "Clean" },
                    { 131, "230", 1, "Clean" },
                    { 132, "231", 1, "Clean" },
                    { 133, "232", 1, "Clean" },
                    { 134, "233", 1, "Clean" },
                    { 135, "234", 1, "Clean" },
                    { 136, "235", 1, "Clean" },
                    { 137, "236", 1, "Clean" },
                    { 138, "237", 1, "Clean" },
                    { 139, "238", 1, "Clean" },
                    { 140, "239", 1, "Clean" },
                    { 141, "240", 1, "Clean" },
                    { 142, "241", 1, "Clean" },
                    { 143, "242", 1, "Clean" },
                    { 144, "243", 1, "Clean" },
                    { 145, "244", 1, "Clean" },
                    { 146, "245", 1, "Clean" },
                    { 147, "246", 1, "Clean" },
                    { 148, "247", 1, "Clean" },
                    { 149, "248", 1, "Clean" },
                    { 150, "249", 1, "Clean" },
                    { 151, "300", 2, "Clean" },
                    { 152, "301", 2, "Clean" },
                    { 153, "302", 2, "Clean" },
                    { 154, "303", 2, "Clean" },
                    { 155, "304", 2, "Clean" },
                    { 156, "305", 2, "Clean" },
                    { 157, "306", 2, "Clean" },
                    { 158, "307", 2, "Clean" },
                    { 159, "308", 2, "Clean" },
                    { 160, "309", 2, "Clean" },
                    { 161, "310", 2, "Clean" },
                    { 162, "311", 2, "Clean" },
                    { 163, "312", 2, "Clean" },
                    { 164, "313", 2, "Clean" },
                    { 165, "314", 2, "Clean" },
                    { 166, "315", 2, "Clean" },
                    { 167, "316", 2, "Clean" },
                    { 168, "317", 2, "Clean" },
                    { 169, "318", 2, "Clean" },
                    { 170, "319", 2, "Clean" },
                    { 171, "320", 2, "Clean" },
                    { 172, "321", 2, "Clean" },
                    { 173, "322", 2, "Clean" },
                    { 174, "323", 2, "Clean" },
                    { 175, "324", 2, "Clean" },
                    { 176, "325", 2, "Clean" },
                    { 177, "326", 2, "Clean" },
                    { 178, "327", 2, "Clean" },
                    { 179, "328", 2, "Clean" },
                    { 180, "329", 2, "Clean" },
                    { 181, "330", 2, "Clean" },
                    { 182, "331", 2, "Clean" },
                    { 183, "332", 2, "Clean" },
                    { 184, "333", 2, "Clean" },
                    { 185, "334", 2, "Clean" },
                    { 186, "335", 2, "Clean" },
                    { 187, "336", 2, "Clean" },
                    { 188, "337", 2, "Clean" },
                    { 189, "338", 2, "Clean" },
                    { 190, "339", 2, "Clean" },
                    { 191, "340", 2, "Clean" },
                    { 192, "341", 2, "Clean" },
                    { 193, "342", 2, "Clean" },
                    { 194, "343", 2, "Clean" },
                    { 195, "344", 2, "Clean" },
                    { 196, "345", 2, "Clean" },
                    { 197, "346", 2, "Clean" },
                    { 198, "347", 2, "Clean" },
                    { 199, "348", 2, "Clean" },
                    { 200, "349", 2, "Clean" },
                    { 201, "350", 2, "Clean" },
                    { 202, "351", 2, "Clean" },
                    { 203, "352", 2, "Clean" },
                    { 204, "353", 2, "Clean" },
                    { 205, "354", 2, "Clean" },
                    { 206, "355", 2, "Clean" },
                    { 207, "356", 2, "Clean" },
                    { 208, "357", 2, "Clean" },
                    { 209, "358", 2, "Clean" },
                    { 210, "359", 2, "Clean" },
                    { 211, "360", 2, "Clean" },
                    { 212, "361", 2, "Clean" },
                    { 213, "362", 2, "Clean" },
                    { 214, "363", 2, "Clean" },
                    { 215, "364", 2, "Clean" },
                    { 216, "365", 2, "Clean" },
                    { 217, "366", 2, "Clean" },
                    { 218, "367", 2, "Clean" },
                    { 219, "368", 2, "Clean" },
                    { 220, "369", 2, "Clean" },
                    { 221, "370", 2, "Clean" },
                    { 222, "371", 2, "Clean" },
                    { 223, "372", 2, "Clean" },
                    { 224, "373", 2, "Clean" },
                    { 225, "374", 2, "Clean" },
                    { 226, "375", 2, "Clean" },
                    { 227, "376", 2, "Clean" },
                    { 228, "377", 2, "Clean" },
                    { 229, "378", 2, "Clean" },
                    { 230, "379", 2, "Clean" },
                    { 231, "380", 2, "Clean" },
                    { 232, "381", 2, "Clean" },
                    { 233, "382", 2, "Clean" },
                    { 234, "383", 2, "Clean" },
                    { 235, "384", 2, "Clean" },
                    { 236, "385", 2, "Clean" },
                    { 237, "386", 2, "Clean" },
                    { 238, "387", 2, "Clean" },
                    { 239, "388", 2, "Clean" },
                    { 240, "389", 2, "Clean" },
                    { 241, "390", 2, "Clean" },
                    { 242, "391", 2, "Clean" },
                    { 243, "392", 2, "Clean" },
                    { 244, "393", 2, "Clean" },
                    { 245, "394", 2, "Clean" },
                    { 246, "395", 2, "Clean" },
                    { 247, "396", 2, "Clean" },
                    { 248, "397", 2, "Clean" },
                    { 249, "398", 2, "Clean" },
                    { 250, "399", 2, "Clean" },
                    { 251, "400", 2, "Clean" },
                    { 252, "401", 2, "Clean" },
                    { 253, "402", 2, "Clean" },
                    { 254, "403", 2, "Clean" },
                    { 255, "404", 2, "Clean" },
                    { 256, "405", 2, "Clean" },
                    { 257, "406", 2, "Clean" },
                    { 258, "407", 2, "Clean" },
                    { 259, "408", 2, "Clean" },
                    { 260, "409", 2, "Clean" },
                    { 261, "410", 2, "Clean" },
                    { 262, "411", 2, "Clean" },
                    { 263, "412", 2, "Clean" },
                    { 264, "413", 2, "Clean" },
                    { 265, "414", 2, "Clean" },
                    { 266, "415", 2, "Clean" },
                    { 267, "416", 2, "Clean" },
                    { 268, "417", 2, "Clean" },
                    { 269, "418", 2, "Clean" },
                    { 270, "419", 2, "Clean" },
                    { 271, "420", 2, "Clean" },
                    { 272, "421", 2, "Clean" },
                    { 273, "422", 2, "Clean" },
                    { 274, "423", 2, "Clean" },
                    { 275, "424", 2, "Clean" },
                    { 276, "425", 2, "Clean" },
                    { 277, "426", 2, "Clean" },
                    { 278, "427", 2, "Clean" },
                    { 279, "428", 2, "Clean" },
                    { 280, "429", 2, "Clean" },
                    { 281, "430", 2, "Clean" },
                    { 282, "431", 2, "Clean" },
                    { 283, "432", 2, "Clean" },
                    { 284, "433", 2, "Clean" },
                    { 285, "434", 2, "Clean" },
                    { 286, "435", 2, "Clean" },
                    { 287, "436", 2, "Clean" },
                    { 288, "437", 2, "Clean" },
                    { 289, "438", 2, "Clean" },
                    { 290, "439", 2, "Clean" },
                    { 291, "440", 2, "Clean" },
                    { 292, "441", 2, "Clean" },
                    { 293, "442", 2, "Clean" },
                    { 294, "443", 2, "Clean" },
                    { 295, "444", 2, "Clean" },
                    { 296, "445", 2, "Clean" },
                    { 297, "446", 2, "Clean" },
                    { 298, "447", 2, "Clean" },
                    { 299, "448", 2, "Clean" },
                    { 300, "449", 2, "Clean" },
                    { 301, "450", 2, "Clean" },
                    { 302, "451", 2, "Clean" },
                    { 303, "452", 2, "Clean" },
                    { 304, "453", 2, "Clean" },
                    { 305, "454", 2, "Clean" },
                    { 306, "455", 2, "Clean" },
                    { 307, "456", 2, "Clean" },
                    { 308, "457", 2, "Clean" },
                    { 309, "458", 2, "Clean" },
                    { 310, "459", 2, "Clean" },
                    { 311, "460", 2, "Clean" },
                    { 312, "461", 2, "Clean" },
                    { 313, "462", 2, "Clean" },
                    { 314, "463", 2, "Clean" },
                    { 315, "464", 2, "Clean" },
                    { 316, "465", 2, "Clean" },
                    { 317, "466", 2, "Clean" },
                    { 318, "467", 2, "Clean" },
                    { 319, "468", 2, "Clean" },
                    { 320, "469", 2, "Clean" },
                    { 321, "470", 2, "Clean" },
                    { 322, "471", 2, "Clean" },
                    { 323, "472", 2, "Clean" },
                    { 324, "473", 2, "Clean" },
                    { 325, "474", 2, "Clean" },
                    { 326, "475", 2, "Clean" },
                    { 327, "476", 2, "Clean" },
                    { 328, "477", 2, "Clean" },
                    { 329, "478", 2, "Clean" },
                    { 330, "479", 2, "Clean" },
                    { 331, "480", 2, "Clean" },
                    { 332, "481", 2, "Clean" },
                    { 333, "482", 2, "Clean" },
                    { 334, "483", 2, "Clean" },
                    { 335, "484", 2, "Clean" },
                    { 336, "485", 2, "Clean" },
                    { 337, "486", 2, "Clean" },
                    { 338, "487", 2, "Clean" },
                    { 339, "488", 2, "Clean" },
                    { 340, "489", 2, "Clean" },
                    { 341, "490", 2, "Clean" },
                    { 342, "491", 2, "Clean" },
                    { 343, "492", 2, "Clean" },
                    { 344, "493", 2, "Clean" },
                    { 345, "494", 2, "Clean" },
                    { 346, "495", 2, "Clean" },
                    { 347, "496", 2, "Clean" },
                    { 348, "497", 2, "Clean" },
                    { 349, "498", 2, "Clean" },
                    { 350, "499", 2, "Clean" },
                    { 351, "500", 3, "Clean" },
                    { 352, "501", 3, "Clean" },
                    { 353, "502", 3, "Clean" },
                    { 354, "503", 3, "Clean" },
                    { 355, "504", 3, "Clean" },
                    { 356, "505", 3, "Clean" },
                    { 357, "506", 3, "Clean" },
                    { 358, "507", 3, "Clean" },
                    { 359, "508", 3, "Clean" },
                    { 360, "509", 3, "Clean" },
                    { 361, "510", 3, "Clean" },
                    { 362, "511", 3, "Clean" },
                    { 363, "512", 3, "Clean" },
                    { 364, "513", 3, "Clean" },
                    { 365, "514", 3, "Clean" },
                    { 366, "515", 3, "Clean" },
                    { 367, "516", 3, "Clean" },
                    { 368, "517", 3, "Clean" },
                    { 369, "518", 3, "Clean" },
                    { 370, "519", 3, "Clean" },
                    { 371, "520", 3, "Clean" },
                    { 372, "521", 3, "Clean" },
                    { 373, "522", 3, "Clean" },
                    { 374, "523", 3, "Clean" },
                    { 375, "524", 3, "Clean" },
                    { 376, "525", 3, "Clean" },
                    { 377, "526", 3, "Clean" },
                    { 378, "527", 3, "Clean" },
                    { 379, "528", 3, "Clean" },
                    { 380, "529", 3, "Clean" },
                    { 381, "530", 3, "Clean" },
                    { 382, "531", 3, "Clean" },
                    { 383, "532", 3, "Clean" },
                    { 384, "533", 3, "Clean" },
                    { 385, "534", 3, "Clean" },
                    { 386, "535", 3, "Clean" },
                    { 387, "536", 3, "Clean" },
                    { 388, "537", 3, "Clean" },
                    { 389, "538", 3, "Clean" },
                    { 390, "539", 3, "Clean" },
                    { 391, "540", 3, "Clean" },
                    { 392, "541", 3, "Clean" },
                    { 393, "542", 3, "Clean" },
                    { 394, "543", 3, "Clean" },
                    { 395, "544", 3, "Clean" },
                    { 396, "545", 3, "Clean" },
                    { 397, "546", 3, "Clean" },
                    { 398, "547", 3, "Clean" },
                    { 399, "548", 3, "Clean" },
                    { 400, "549", 3, "Clean" }
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
                name: "IX_BookingServices_ServiceId",
                table: "BookingServices",
                column: "ServiceId");

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
