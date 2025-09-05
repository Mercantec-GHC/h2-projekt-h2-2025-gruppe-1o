using API.Data;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Indeholder endpoints til administrative formål under udvikling.
    /// </summary>
    /// <remarks>
    ///  **VIGTIGT:** Denne controller er kun beregnet til brug i et udviklingsmiljø.
    /// Alle endpoints er beskyttet til kun at kunne kaldes af brugere med rollerne 'Manager' eller 'Admin'.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager, Admin")] // Låst til de højeste roller
    public class DataSeederController : ControllerBase
    {
        private readonly DataSeederService _seederService;
        private readonly IWebHostEnvironment _environment;
        private readonly AppDBContext _context;
        private readonly ILogger<DataSeederController> _logger;

        /// <summary>
        /// Initialiserer en ny instans af DataSeederController.
        /// </summary>
        public DataSeederController(DataSeederService seederService, IWebHostEnvironment environment, AppDBContext context, ILogger<DataSeederController> logger)
        {
            _seederService = seederService;
            _environment = environment;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Udfylder databasen med et antal genererede brugere og bookinger.
        /// </summary>
        /// <remarks>
        /// Denne operation kan kun udføres i et **Development**-miljø.
        /// Bruger Bogus-biblioteket til at skabe realistiske testdata.
        /// </remarks>
        /// <param name="userCount">Antallet af brugere, der skal genereres. Standard er 50.</param>
        /// <param name="bookingCount">Det cirka antal bookinger, der skal genereres. Standard er 200.</param>
        /// <returns>En bekræftelsesmeddelelse om, at databasen er blevet seedet.</returns>
        [HttpPost("seed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SeedDatabase([FromQuery] int userCount = 50, [FromQuery] int bookingCount = 200)
        {
            if (!_environment.IsDevelopment())
            {
                _logger.LogWarning("Sikkerhedsbrud: Forsøg på at seede data i et ikke-udviklingsmiljø ({EnvironmentName}).", _environment.EnvironmentName);
                return Forbid("Database seeding er kun tilladt i development miljø.");
            }

            try
            {
                await _seederService.SeedDataAsync(userCount, bookingCount);
                return Ok(new { message = $"Database seeded succesfuldt med {userCount} brugere og {bookingCount} (ca.) bookinger." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl opstod under database seeding.");
                return StatusCode(500, "En fejl opstod under seeding af databasen.");
            }
        }

        /// <summary>
        /// Sletter alle genererede brugere og bookinger fra databasen.
        /// </summary>
        /// <remarks>
        /// Denne operation kan kun udføres i et **Development**-miljø. Handlingen kan ikke fortrydes.
        /// </remarks>
        /// <returns>En bekræftelsesmeddelelse om, at data er blevet slettet.</returns>
        [HttpDelete("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearDatabase()
        {
            if (!_environment.IsDevelopment())
            {
                _logger.LogWarning("Sikkerhedsbrud: Forsøg på at rydde data i et ikke-udviklingsmiljø ({EnvironmentName}).", _environment.EnvironmentName);
                return Forbid("Database rydning er kun tilladt i development miljø.");
            }

            try
            {
                await _seederService.ClearDatabaseAsync();
                return Ok(new { message = "Genererede brugere og bookinger er blevet slettet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl opstod under rydning af database.");
                return StatusCode(500, "En fejl opstod under rydning af databasen.");
            }
        }

        /// <summary>
        /// Henter statistik over antallet af rækker i centrale tabeller.
        /// </summary>
        /// <remarks>
        /// Denne operation kan kun udføres i et **Development**-miljø.
        /// </remarks>
        /// <returns>Et objekt med antallet af brugere, bookinger, værelser, værelsestyper og roller.</returns>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDatabaseStats()
        {
            if (!_environment.IsDevelopment())
            {
                return Forbid("Statistik er kun tilgængelig i development miljø.");
            }

            var stats = new
            {
                UserCount = await _context.Users.CountAsync(),
                BookingCount = await _context.Bookings.CountAsync(),
                RoomCount = await _context.Rooms.CountAsync(),
                RoomTypeCount = await _context.RoomTypes.CountAsync(),
                RoleCount = await _context.Roles.CountAsync()
            };

            return Ok(stats);
        }
    }
}