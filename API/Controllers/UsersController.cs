using API.Data;
using API.Services;
using DomainModels;
using DomainModels.DTOs;
using DomainModels.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<UsersController> _logger;
        private readonly LoginAttemptService _loginAttemptService;

        public UsersController(AppDBContext context, JwtService jwtService, ILogger<UsersController> logger, LoginAttemptService loginAttemptService)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _loginAttemptService = loginAttemptService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDetailDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserUpdateDto userDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Bruger med ID {CurrentUserId} forsøger at opdatere bruger med ID {TargetId}", currentUserId, id);

            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                _logger.LogWarning("Opdatering fejlede: Bruger med ID {TargetId} blev ikke fundet.", id);
                return NotFound();
            }

            // Autorisationstjek: Er den indloggede bruger den samme som den, der skal opdateres?
            if (userToUpdate.Id != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("FORBIDDEN: Bruger {CurrentUserId} har ikke tilladelse til at opdatere bruger {TargetId}.", currentUserId, id);
                return Forbid();
            }

            _logger.LogInformation("Bruger {CurrentUserId} har tilladelse. Opdaterer felter...", currentUserId);
            userToUpdate.FirstName = userDto.FirstName;
            userToUpdate.LastName = userDto.LastName;
            userToUpdate.Email = userDto.Email;
            userToUpdate.PhoneNumber = userDto.PhoneNumber ?? string.Empty;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Bruger {TargetId} blev opdateret succesfuldt i databasen.", id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DATABASE FEJL: Kunne ikke gemme ændringer for bruger {TargetId}.", id);
                return StatusCode(500, "Der opstod en fejl under lagring til databasen.");
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Bruger ikke fundet.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.HashedPassword))
            {
                return BadRequest("Den nuværende adgangskode er ikke korrekt.");
            }

            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.HashedPassword = newHashedPassword;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Adgangskode blev opdateret succesfuldt." });
        }

        // ... Resten af metoderne (Register, Login osv.) er uændrede ...
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("En bruger med denne email findes allerede.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null) throw new InvalidOperationException("Systemkonfigurationsfejl: Standard brugerrolle 'User' mangler.");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = dto.Email,
                HashedPassword = hashedPassword,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                RoleId = userRole.Id,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bruger oprettet!", userId = user.Id });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
            {
                return Unauthorized("Forkert email eller adgangskode");
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new { id = user.Id, email = user.Email, firstName = user.FirstName, lastName = user.LastName, role = user.Role?.Name ?? "User" }
            });
        }
    }
}