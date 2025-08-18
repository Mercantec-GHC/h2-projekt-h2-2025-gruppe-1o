using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainModels;

namespace API.Services
{
    /// <summary>
    /// Service til håndtering af JWT tokens, herunder generering og validering.
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        /// <summary>
        /// Initialiserer JwtService ved at indlæse indstillinger fra app-konfigurationen.
        /// </summary>
        /// <param name="configuration">Applikationens konfigurations-provider.</param>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789";

            _issuer = _configuration["Jwt:Issuer"]
            ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? "H2-2025-API";

            _audience = _configuration["Jwt:Audience"]
            ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? "H2-2025-Client";

            _expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"]
            ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
            ?? "60");
        }

        /// <summary>
        /// Genererer et nyt JWT-token for en given bruger.
        /// </summary>
        /// <param name="user">Brugerobjektet, som tokenet skal genereres for.</param>
        /// <returns>En JWT-token som en streng.</returns>
        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("userId", user.Id),
                new Claim("username", user.Username),
                new Claim("email", user.Email)
            };

            // Tilføj rolle claim hvis brugeren har en rolle
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
                claims.Add(new Claim("role", user.Role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}