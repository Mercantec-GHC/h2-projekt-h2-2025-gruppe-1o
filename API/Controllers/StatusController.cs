using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Leverer simple status-endpoints til at overvåge API'ens helbred.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Tjekker om API'en kører korrekt.
        /// </summary>
        /// <returns>Status og besked om API'ens tilstand.</returns>
        /// <response code="200">API'en er kørende.</response>
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "OK", message = "API'en er kørende!" });
        }

        /// <summary>
        /// Simulerer et tjek af databaseforbindelsen (erstattes senere med et reelt tjek).
        /// </summary>
        /// <returns>Status og besked om databaseforbindelse.</returns>
        /// <response code="200">Returnerer status for databaseforbindelsen.</response>
        [HttpGet("dbhealthcheck")]
        public IActionResult DBHealthCheck()
        {
            // Indtil vi har opsat EFCore, returnerer vi bare en besked
            try
            {
                // using (var context = new ApplicationDbContext())
                // {
                //     context.Database.CanConnect();
                // }
                throw new Exception("I har endnu ikke lært at opsætte EFCore! Det kommer senere!");
            }
            catch (Exception ex)
            {
                return Ok(new { status = "Error", message = "Fejl ved forbindelse til database: " + ex.Message });
            }
            return Ok(new { status = "OK", message = "Database er kørende!" });
        }

        /// <summary>
        /// Simpelt ping-endpoint til at teste om API'en svarer.
        /// </summary>
        /// <returns>Status og en "Pong" besked.</returns>
        /// <response code="200">API'en svarede med Pong.</response>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }
    }
}