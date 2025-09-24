using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager, Admin")] // Sikrer at kun autoriserede kan teste
    public class MailController : ControllerBase
    {
        private readonly MailService _mailService;
        private readonly ILogger<MailController> _logger;

        public MailController(MailService mailService, ILogger<MailController> logger)
        {
            _mailService = mailService;
            _logger = logger;
        }

        /// <summary>
        /// Sender en simpel test-e-mail til en specificeret modtager.
        /// </summary>
        /// <param name="toEmail">Modtagerens e-mailadresse.</param>
        /// <returns>En statusmeddelelse om, hvorvidt afsendelsen lykkedes.</returns>
        [HttpPost("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTestEmail([FromBody] string toEmail)
        {
            if (string.IsNullOrEmpty(toEmail) || !toEmail.Contains('@'))
            {
                return BadRequest("Angiv venligst en gyldig e-mailadresse.");
            }

            var subject = "Test af MailService fra Flyhigh Hotel API";
            var body = $"<h1>Dette er en test</h1><p>Hvis du modtager denne e-mail, er din MailService konfigureret korrekt.</p><p>Tidspunkt: {DateTime.UtcNow:G}</p>";

            _logger.LogInformation("Påbegynder afsendelse af test-mail til {Email}", toEmail);

            var success = await _mailService.SendEmailAsync(toEmail, subject, body);

            if (success)
            {
                return Ok(new { message = $"Test-mail sendt succesfuldt til {toEmail}." });
            }

            return StatusCode(500, new { message = $"Fejl: Kunne ikke sende test-mail til {toEmail}. Tjek API-loggen for detaljer." });
        }
    }
}