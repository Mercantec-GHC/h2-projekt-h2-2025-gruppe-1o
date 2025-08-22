using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Indeholder diagnostiske endpoints til at verificere API'ens status og teste specifikke funktioner.
    /// </summary>
    /// <remarks>
    /// Denne controller er offentligt tilgængelig og bruges primært til sundhedstjek,
    /// debugging og til at verificere, at systemkomponenter som fejlhåndtering fungerer korrekt.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;

        /// <summary>
        /// Initialiserer en ny instans af StatusController.
        /// </summary>
        /// <param name="logger">Logger til at logge information og fejl.</param>
        public StatusController(ILogger<StatusController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Udfører et simpelt sundhedstjek af API'en.
        /// </summary>
        /// <remarks>
        /// Dette endpoint bruges typisk af eksterne monitoreringsværktøjer (f.eks. en load balancer)
        /// til at bekræfte, at applikationen er i live og kan modtage requests.
        /// </remarks>
        /// <returns>En statusmeddelelse, der indikerer, at API'en kører.</returns>
        /// <response code="200">API'en er kørende og svarer korrekt.</response>
        [HttpGet("healthcheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            _logger.LogInformation("HealthCheck endpoint blev kaldt kl. {Timestamp}", DateTime.UtcNow);
            return Ok(new { status = "OK", message = "API'en er kørende!" });
        }

        /// <summary>
        /// Et simpelt "ping-pong" endpoint til hurtig forbindelse-test.
        /// </summary>
        /// <returns>Et "Pong" svar for at bekræfte, at controlleren er aktiv.</returns>
        /// <response code="200">Returnerer en "Pong" besked.</response>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            _logger.LogInformation("Ping endpoint blev kaldt.");
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }

        /// <summary>
        /// Genererer bevidst en intern serverfejl.
        /// </summary>
        /// <remarks>
        /// Dette endpoint er designet til at kaste en undtagelse for at teste,
        /// om den globale fejlhåndterings-middleware er konfigureret korrekt og fanger 500-fejl.
        /// Det forventede svar er en HTTP 500 Internal Server Error.
        /// </remarks>
        /// <returns>Ingenting. Vil altid resultere i en 500-fejl.</returns>
        /// <response code="500">Dette er den forventede respons, da endpointet kaster en exception.</response>
        [HttpGet("test-error")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult TestError()
        {
            _logger.LogInformation("Kører bevidst test-fejl-endpoint.");
            throw new Exception("Dette er en bevidst test-fejl for at verificere den globale handler!");
        }

        /// <summary>
        /// Udfører en isoleret test af BCrypt-verifikationslogikken.
        /// </summary>
        /// <remarks>
        /// Bruges til at debugge login-problemer ved at verificere, om et kendt password
        /// matcher et kendt hash. Dette sker uafhængigt af database og bruger-input.
        /// </remarks>
        /// <returns>Et objekt, der viser testens input og resultat.</returns>
        /// <response code="200">Returnerer resultatet af BCrypt-verifikationen.</response>
        [HttpGet("test-bcrypt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult TestBcrypt()
        {
            string passwordFromUser = "Password123!";
            string hashFromDb = "$2a$11$jCvV3t1G2u2AL.26A72Gv.ECi1G93olRzSP4i3.eIh3Kx/p2yvD.W";

            _logger.LogInformation("--- Kører BCrypt Isolationstest ---");
            _logger.LogInformation("Test Password: '{password}'", passwordFromUser);
            _logger.LogInformation("Test Hash: '{hash}'", hashFromDb);

            bool result = BCrypt.Net.BCrypt.Verify(passwordFromUser, hashFromDb);

            _logger.LogInformation("Resultat af isoleret BCrypt.Verify: {result}", result);

            return Ok(new
            {
                passwordUsed = passwordFromUser,
                hashUsed = hashFromDb,
                isMatch = result
            });
        }
    }
}