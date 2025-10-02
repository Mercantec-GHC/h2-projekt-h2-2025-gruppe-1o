using ActiveDirectoryTesting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Tilbyder endpoints til at administrere og vise data fra Active Directory.
    /// </summary>
    /// <remarks>
    /// Denne controller er beskyttet og kræver 'Manager'-rollen for adgang.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class ActiveDirectoryController : ControllerBase
    {
        private readonly ActiveDirectoryService _adService;
        private readonly ILogger<ActiveDirectoryController> _logger;

        /// <summary>
        /// Initialiserer en ny instans af ActiveDirectoryController.
        /// </summary>
        public ActiveDirectoryController(ActiveDirectoryService adService, ILogger<ActiveDirectoryController> logger)
        {
            _adService = adService;
            _logger = logger;
        }

        /// <summary>
        /// Henter en liste over alle brugere fra Active Directory.
        /// </summary>
        /// <returns>En sorteret liste af AD-brugere.</returns>
        /// <response code="200">Returnerer en liste af brugere.</response>
        /// <response code="401">Brugeren er ikke autoriseret.</response>
        /// <response code="403">Brugeren har ikke den påkrævede 'Manager'-rolle.</response>
        /// <response code="500">Hvis der opstår en fejl under kommunikation med Active Directory.</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<ADUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _adService.GetAllUsers();
                // Vi sorterer resultatet for en konsistent og brugervenlig visning i frontend.
                return Ok(users.OrderBy(u => u.DisplayName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under hentning af alle brugere fra Active Directory.");
                return StatusCode(500, "Der opstod en intern fejl ved kommunikation med Active Directory.");
            }
        }

        /// <summary>
        /// --- START ÆNDRING ---
        /// Henter en liste over grupper fra Active Directory, der har mindst ét medlem.
        /// --- SLUT ÆNDRING ---
        /// </summary>
        /// <returns>En sorteret liste af AD-grupper med medlemmer.</returns>
        /// <response code="200">Returnerer en liste af grupper.</response>
        /// <response code="401">Brugeren er ikke autoriseret.</response>
        /// <response code="403">Brugeren har ikke den påkrævede 'Manager'-rolle.</response>
        /// <response code="500">Hvis der opstår en fejl under kommunikation med Active Directory.</response>
        [HttpGet("groups")]
        [ProducesResponseType(typeof(IEnumerable<ADGroup>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetGroups()
        {
            try
            {
                // --- START ÆNDRING ---
                // Vi tilføjer parameteret 'hasMembers: true' for kun at hente grupper med medlemmer.
                var groups = _adService.AdvancedGroupSearch(hasMembers: true);
                // --- SLUT ÆNDRING ---
                return Ok(groups.OrderBy(g => g.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under hentning af grupper fra Active Directory.");
                return StatusCode(500, "Der opstod en intern fejl ved kommunikation med Active Directory.");
            }
        }
    }
}