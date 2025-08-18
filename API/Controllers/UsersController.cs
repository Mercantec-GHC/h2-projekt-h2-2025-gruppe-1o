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
    /// <summary>
    /// Håndterer brugerregistrering, login og brugeradministration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;

        /// <summary>
        /// Initialiserer en ny instans af UsersController.
        /// </summary>
        /// <param name="context">Database context for bruger-systemet.</param>
        /// <param name="jwtService">Service til at generere JWT tokens.</param>
        public UsersController(AppDBContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Henter en liste over alle brugere i systemet. (Kun for administratorer)
        /// </summary>
        /// <returns>En liste af brugere med deres roller.</returns>
        /// <response code="200">Returnerer listen af brugere.</response>
        /// <response code="401">Hvis anmodningen ikke kommer fra en logget-ind bruger.</response>
        /// <response code="403">Hvis den indloggede bruger ikke har rollen 'Admin'.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Select(user => new UserGetDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role != null ? user.Role.Name : "N/A"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Henter en specifik bruger ud fra ID.
        /// </summary>
        /// <param name="id">Brugerens unikke ID.</param>
        /// <returns>Brugerens detaljer.</returns>
        /// <response code="200">Returnerer brugerens detaljer.</response>
        /// <response code="404">Hvis en bruger med det angivne ID ikke blev fundet.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDto>> GetUser(string id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return UserMapping.ToUserGetDto(user);
        }

        /// <summary>
        /// Opdaterer en brugers information. En bruger kan opdatere sig selv, en admin kan opdatere alle.
        /// </summary>
        /// <param name="id">ID'et på den bruger, der skal opdateres.</param>
        /// <param name="userDto">De nye brugeroplysninger (brugernavn og email).</param>
        /// <returns>Ingen indhold ved succes.</returns>
        /// <response code="204">Brugeren blev opdateret succesfuldt.</response>
        /// <response code="403">Hvis en bruger forsøger at opdatere en anden bruger uden at være admin.</response>
        /// <response code="404">Hvis brugeren med det angivne ID ikke findes.</response>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserUpdateDto userDto)
        {
            var userToUpdate = await _context.Users.FindAsync(id);

            if (userToUpdate == null)
            {
                return NotFound("Brugeren blev ikke fundet.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userToUpdate.Id != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("Du har ikke tilladelse til at opdatere denne bruger.");
            }

            userToUpdate.Username = userDto.Username;
            userToUpdate.Email = userDto.Email;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Registrerer en ny bruger i systemet.
        /// </summary>
        /// <param name="dto">Data for den nye bruger, inklusiv email, brugernavn og adgangskode.</param>
        /// <returns>En bekræftelse på, at brugeren er oprettet.</returns>
        /// <response code="200">Returnerer en succesbesked og det nye bruger-ID.</response>
        /// <response code="400">Hvis en bruger med den angivne email allerede eksisterer.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("En bruger med denne email findes allerede.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            if (userRole == null)
                return StatusCode(500, "Standard brugerrolle 'User' blev ikke fundet i databasen.");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = dto.Email,
                HashedPassword = hashedPassword,
                Username = dto.Username,
                PasswordBackdoor = dto.Password,
                RoleId = userRole.Id,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bruger oprettet!", userId = user.Id });
        }

        /// <summary>
        /// Logger en eksisterende bruger ind og returnerer et JWT-token.
        /// </summary>
        /// <param name="dto">Login-oplysninger (email og adgangskode).</param>
        /// <returns>Et JWT-token og grundlæggende brugerinformation.</returns>
        /// <response code="200">Returnerer token og brugerinfo ved succesfuldt login.</response>
        /// <response code="401">Hvis email eller adgangskode er forkert.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
                return Unauthorized("Forkert email eller adgangskode");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    username = user.Username,
                    role = user.Role?.Name ?? "User"
                }
            });
        }

        /// <summary>
        /// Henter detaljer om den aktuelt indloggede bruger baseret på deres JWT-token.
        /// </summary>
        /// <returns>Detaljeret information om den indloggede bruger.</returns>
        /// <response code="200">Returnerer brugerens detaljer.</response>
        /// <response code="401">Hvis der ikke er et gyldigt token i anmodningen.</response>
        /// <response code="404">Hvis brugeren fra tokenet ikke længere eksisterer i databasen.</response>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                Role = user.Role?.Name ?? "User"
            });
        }

        /// <summary>
        /// Sletter en bruger fra systemet. (Kun for administratorer)
        /// </summary>
        /// <param name="id">ID'et på den bruger, der skal slettes.</param>
        /// <returns>Ingen indhold ved succes.</returns>
        /// <response code="204">Brugeren blev slettet succesfuldt.</response>
        /// <response code="403">Hvis den indloggede bruger ikke er admin.</response>
        /// <response code="404">Hvis brugeren med det angivne ID ikke findes.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Tildeler en ny rolle til en bruger. (Kun for administratorer)
        /// </summary>
        /// <param name="id">ID'et på brugeren, der skal have en ny rolle.</param>
        /// <param name="dto">Objekt, der indeholder ID'et på den nye rolle.</param>
        /// <returns>En bekræftelsesbesked.</returns>
        /// <response code="200">Returnerer en succesbesked.</response>
        /// <response code="400">Hvis det angivne rolle-ID er ugyldigt.</response>
        /// <response code="404">Hvis brugeren ikke findes.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> AssignUserRole(string id, [FromBody] AssignRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Bruger ikke fundet.");

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null) return BadRequest("Ugyldig rolle.");

            user.RoleId = dto.RoleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Rollen '{role.Name}' blev tildelt til bruger {user.Email}." });
        }

        /// <summary>
        /// Henter en liste over alle tilgængelige brugerroller. (Kun for administratorer)
        /// </summary>
        /// <returns>En liste af roller med ID, navn og beskrivelse.</returns>
        /// <response code="200">Returnerer listen af roller.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public async Task<ActionResult<object>> GetAvailableRoles()
        {
            return Ok(await _context.Roles
                .Select(r => new { id = r.Id, name = r.Name, description = r.Description })
                .ToListAsync());
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
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