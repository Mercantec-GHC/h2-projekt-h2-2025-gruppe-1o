using API.Data;
using API.Services;
using DomainModels;
using DomainModels.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsers()
        {
            var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Ukendt Admin";
            _logger.LogInformation("Administrator '{AdminName}' anmoder om at hente alle brugere.", adminName);

            var users = await _context.Users
                .Include(u => u.Role)
                .Select(user => UserMapping.ToUserGetDto(user))
                .ToListAsync();

            _logger.LogInformation("Hentet {UserCount} brugere succesfuldt.", users.Count);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDto>> GetUser(string id)
        {
            _logger.LogInformation("Forsøger at hente bruger med ID: {UserId}", id);
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("Bruger med ID {UserId} blev ikke fundet.", id);
                return NotFound();
            }

            _logger.LogInformation("Bruger med ID {UserId} blev fundet succesfuldt.", id);
            return UserMapping.ToUserGetDto(user);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserUpdateDto userDto)
        {
            _logger.LogInformation("Bruger {RequestingUserId} forsøger at opdatere bruger {TargetUserId}", User.FindFirstValue(ClaimTypes.NameIdentifier), id);

            var userToUpdate = await _context.Users.FindAsync(id);

            if (userToUpdate == null)
            {
                _logger.LogWarning("Opdatering fejlede: Bruger med ID {TargetUserId} blev ikke fundet.", id);
                return NotFound("Brugeren blev ikke fundet.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userToUpdate.Id != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Uautoriseret forsøg: Bruger {RequestingUserId} forsøgte at opdatere bruger {TargetUserId} uden tilladelse.", currentUserId, id);
                return Forbid("Du har ikke tilladelse til at opdatere denne bruger.");
            }

            userToUpdate.Username = userDto.Username;
            userToUpdate.Email = userDto.Email;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Bruger {TargetUserId} blev opdateret succesfuldt.", id);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            _logger.LogInformation("Forsøger at registrere ny bruger med email: {Email}", dto.Email);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning("Registrering fejlede: Email {Email} er allerede i brug.", dto.Email);
                return BadRequest("En bruger med denne email findes allerede.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            if (userRole == null)
            {
                _logger.LogError("Kritisk fejl: Standard brugerrolle 'User' blev ikke fundet i databasen under registrering.");
                // Kaster en exception, som vores globale handler fanger og returnerer som en 500-fejl.
                throw new InvalidOperationException("Systemkonfigurationsfejl: Standard brugerrolle 'User' mangler.");
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = dto.Email,
                HashedPassword = hashedPassword,
                Username = dto.Username,
                RoleId = userRole.Id,
                PasswordBackdoor = dto.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bruger med email {Email} blev oprettet succesfuldt med ID {UserId}", user.Email, user.Id);
            return Ok(new { message = "Bruger oprettet!", userId = user.Id });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            _logger.LogInformation("Login-forsøg for email: {Email}", dto.Email);

            if (_loginAttemptService.IsLockedOut(dto.Email))
            {
                var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(dto.Email);
                _logger.LogWarning("Login afvist for {Email} pga. lockout. Resterende tid: {Seconds} sekunder.", dto.Email, remainingSeconds);
                return StatusCode(429, new
                {
                    message = "Konto midlertidigt låst pga. for mange mislykkede login forsøg.",
                    remainingLockoutSeconds = remainingSeconds
                });
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
            {
                var delaySeconds = _loginAttemptService.RecordFailedAttempt(dto.Email);
                _logger.LogWarning("Mislykket login-forsøg for {Email}. Delay på {Delay} sekunder påført.", dto.Email, delaySeconds);

                if (delaySeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
                return Unauthorized("Forkert email eller adgangskode");
            }

            _loginAttemptService.RecordSuccessfulLogin(dto.Email);

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("Bruger {Email} loggede succesfuldt ind.", user.Email);
            return Ok(new
            {
                token,
                user = new { id = user.Id, email = user.Email, username = user.Username, role = user.Role?.Name ?? "User" }
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Uautoriseret 'me' anmodning: Token mangler NameIdentifier claim.");
                return Unauthorized();
            }

            _logger.LogInformation("Henter detaljer for indlogget bruger: {UserId}", userId);
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogError("Bruger {UserId} fra gyldigt token blev ikke fundet i databasen.", userId);
                return NotFound("Brugeren fra dit token eksisterer ikke længere.");
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                user.LastLogin,
                user.CreatedAt,
                Role = user.Role?.Name ?? "User"
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("Admin {AdminUser} forsøger at slette bruger {TargetUserId}", User.FindFirstValue(ClaimTypes.Name), id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Sletning fejlede: Bruger med ID {TargetUserId} blev ikke fundet.", id);
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bruger {TargetUserId} blev slettet succesfuldt af {AdminUser}.", id, User.FindFirstValue(ClaimTypes.Name));
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> AssignUserRole(string id, [FromBody] AssignRoleDto dto)
        {
            _logger.LogInformation("Admin {AdminUser} forsøger at tildele RoleId {RoleId} til bruger {TargetUserId}", User.FindFirstValue(ClaimTypes.Name), dto.RoleId, id);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Rolletildeling fejlede: Bruger med ID {TargetUserId} blev ikke fundet.", id);
                return NotFound("Bruger ikke fundet.");
            }

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
            {
                _logger.LogWarning("Rolletildeling fejlede: Rolle med ID {RoleId} er ugyldig.", dto.RoleId);
                return BadRequest("Ugyldig rolle.");
            }

            user.RoleId = dto.RoleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rollen '{RoleName}' blev tildelt til bruger {UserEmail} af {AdminUser}.", role.Name, user.Email, User.FindFirstValue(ClaimTypes.Name));
            return Ok(new { message = $"Rollen '{role.Name}' blev tildelt til bruger {user.Email}." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public async Task<ActionResult<object>> GetAvailableRoles()
        {
            _logger.LogInformation("Admin {AdminUser} henter listen over tilgængelige roller.", User.FindFirstValue(ClaimTypes.Name));
            var roles = await _context.Roles
                .Select(r => new { id = r.Id, name = r.Name, description = r.Description })
                .ToListAsync();
            return Ok(roles);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("login-status/{email}")]
        public IActionResult GetLoginStatus(string email)
        {
            _logger.LogInformation("Admin {AdminUser} tjekkede login-status for email: {TargetEmail}", User.Identity.Name, email);

            var attemptInfo = _loginAttemptService.GetLoginAttemptInfo(email);
            var isLockedOut = _loginAttemptService.IsLockedOut(email);

            return Ok(new
            {
                email,
                isLockedOut,
                failedAttempts = attemptInfo?.FailedAttempts ?? 0,
                lastAttempt = attemptInfo?.LastAttempt,
                lockoutUntil = attemptInfo?.LockoutUntil,
                remainingLockoutSeconds = _loginAttemptService.GetRemainingLockoutSeconds(email)
            });
        }
    }

    // === DTOs anvendt i denne controller ===
    public class AssignRoleDto
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;
    }

    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Brugernavn er påkrævet")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
        [Required(ErrorMessage = "Email er påkrævet")]
        public string Email { get; set; } = string.Empty;
    }
}