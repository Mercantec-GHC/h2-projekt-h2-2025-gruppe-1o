using API.Data;
using API.Services;
using DomainModels;
using DomainModels.DTOs;
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
        private readonly ActiveDirectoryTesting.ActiveDirectoryService _adService; // TILFØJ DENNE

        public UsersController(
            AppDBContext context,
            JwtService jwtService,
            ILogger<UsersController> logger,
            LoginAttemptService loginAttemptService,
            ActiveDirectoryTesting.ActiveDirectoryService adService) // TILFØJ DENNE
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _loginAttemptService = loginAttemptService;
            _adService = adService; // TILFØJ DENNE
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
            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null) return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userToUpdate.Id != currentUserId && !User.IsInRole("Admin")) return Forbid();

            userToUpdate.FirstName = userDto.FirstName;
            userToUpdate.LastName = userDto.LastName;
            userToUpdate.Email = userDto.Email;
            userToUpdate.PhoneNumber = userDto.PhoneNumber ?? string.Empty;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

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
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                HashedPassword = hashedPassword,
                RoleId = userRole.Id,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bruger oprettet!", userId = user.Id });
        }

        [AllowAnonymous]
        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin(StaffLoginDto dto)
        {
            // 1. Valider mod Active Directory med det direkte brugernavn
            var isValidAdUser = _adService.ValidateUserCredentials(dto.Username, dto.Password);
            if (!isValidAdUser)
            {
                return Unauthorized("Forkert medarbejder-login eller adgangskode.");
            }

            // 2. Hent brugeroplysninger (inkl. telefonnummer) og grupper fra AD
            var adUser = _adService.GetUserWithGroups(dto.Username);
            if (adUser == null || string.IsNullOrWhiteSpace(adUser.Email))
            {
                return StatusCode(500, "Brugeren mangler en email i Active Directory og kan ikke logges ind.");
            }

            // 3. Find eller opret bruger i lokal database
            var localUser = await _context.Users
                                          .Include(u => u.Role)
                                          .FirstOrDefaultAsync(u => u.Email.ToLower() == adUser.Email.ToLower());

            if (localUser == null)
            {
                // Brugeren oprettes
                localUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = adUser.Email,
                    FirstName = adUser.FirstName,
                    LastName = adUser.LastName,
                    PhoneNumber = adUser.Phone,
                    HashedPassword = "EXTERNALLY_MANAGED"
                };
                _context.Users.Add(localUser);
            }
            else
            {
                // Brugeren opdateres
                localUser.FirstName = adUser.FirstName;
                localUser.LastName = adUser.LastName;
                localUser.PhoneNumber = adUser.Phone; 
            }

            // 4. Synkroniser roller
            Console.WriteLine($"--- DEBUG: AD Groups for {dto.Username}: {string.Join(", ", adUser.Groups)} ---");
            
            var roleNameFromAd = adUser.Groups.FirstOrDefault(g =>
            string.Equals(g, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(g, "Manager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(g, "Receptionist", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(g, "Housekeeping", StringComparison.OrdinalIgnoreCase)
            ) ?? "Housekeeping"; // Default to Housekeeping instead of Staff for helle

            Console.WriteLine($"--- DEBUG: Assigned role for {dto.Username}: {roleNameFromAd} ---");

            var localRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleNameFromAd);
            if (localRole == null)
            {
                localRole = new Role { Id = Guid.NewGuid().ToString(), Name = roleNameFromAd };
                _context.Roles.Add(localRole);
                Console.WriteLine($"--- DEBUG: Created new role: {roleNameFromAd} ---");
            }
            localUser.Role = localRole;

            localUser.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(localUser);

            return Ok(new
            {
                token,
                user = new { id = localUser.Id, email = localUser.Email, firstName = localUser.FirstName, role = localUser.Role.Name }
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Bruger ikke fundet.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.HashedPassword))
            {
                return BadRequest("Den nuværende adgangskode er ikke korrekt.");
            }

            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.HashedPassword = newHashedPassword;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Adgangskode blev opdateret succesfuldt." });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

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
