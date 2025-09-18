using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddLongDescriptionToRoomType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "RoomTypes",
                newName: "ShortDescription");

            migrationBuilder.AddColumn<string>(
                name: "LongDescription",
                table: "RoomTypes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LongDescription", "ShortDescription" },
                values: new object[] { "Vores Standard Værelse byder på 25 velindrettede kvadratmeter med en luksuriøs queen-size seng, skrivebord og et moderne badeværelse med regnbruser. Nyd faciliteter som high-speed WiFi, et 4K Smart TV med streaming, og en minibar. Værelset er designet med rene linjer og en rolig farvepalette for at sikre et afslappende ophold.", "Et elegant og komfortabelt værelse, perfekt til forretningsrejsen eller en weekendtur." });

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LongDescription", "ShortDescription" },
                values: new object[] { "Vores Deluxe Suite på 55 kvadratmeter er indbegrebet af moderne luksus. Suiten har et separat soveværelse med en king-size seng og en rummelig opholdsstue med sofaarrangement og spiseplads. Fra de store panoramavinduer har du en fantastisk udsigt over byen. Badeværelset er udstyret med både badekar og en separat regnbruser, samt eksklusive toiletartikler.", "Oplev ekstra plads og luksus med en separat opholdsstue og en imponerende udsigt." });

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "LongDescription", "ShortDescription" },
                values: new object[] { "Presidential Suiten er mere end et værelse; det er indbegrebet af kompromisløs luksus og Flyhigh Hotels mest prestigefyldte residens. Træd ind i en verden af raffineret elegance, hvor 120 kvadratmeter er dedikeret til din absolutte komfort.\r\n\r\nDen ekspansive opholdsstue er et statement i sig selv med nøje udvalgt kunst, designer-møbler og et imponerende flygel. Gulv-til-loft-vinduer bader rummet i naturligt lys.\r\n\r\nSuiten råder over to separate soveværelser, hver med luksuriøse king-size senge og tilhørende spa-lignende marmorbadeværelser. Det fuldt udstyrede gourmetkøkken giver desuden mulighed for, at vores private kok kan kreere skræddersyede kulinariske oplevelser direkte i suiten.\r\n", "Den ultimative luksusoplevelse fordelt på 120 kvadratmeter med eksklusiv service." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongDescription",
                table: "RoomTypes");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "RoomTypes",
                newName: "Description");

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Hyggeligt enkeltværelse med alt hvad du behøver.");

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Rummelig suite med panoramaudsigt og premium faciliteter.");

            migrationBuilder.UpdateData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Den ultimative luksusoplevelse med eksklusiv service.");
        }
    }
}
