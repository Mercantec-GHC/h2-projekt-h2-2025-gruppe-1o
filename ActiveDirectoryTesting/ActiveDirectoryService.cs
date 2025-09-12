using Microsoft.Extensions.Configuration; // Sørg for at have denne using-statement please virk
using System.Configuration;
using System.DirectoryServices.AccountManagement;

namespace ActiveDirectoryTesting
{
    // Dine ADUser, ADGroup og ADConfig klasser skal defineres her ELLER i en separat fil.
    public class ADGroup
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new List<string>();
    }

    public class ADUser
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public DateTime? LastLogon { get; set; }
        public DateTime? PasswordLastSet { get; set; }
        public bool IsEnabled { get; set; } = true;
        public List<string> Groups { get; set; } = new List<string>();
    }

    public class ADConfig
    {
        public string Server { get; set; } = "10.133.71.101";
        public string Username { get; set; } = "hans";
        public string Password { get; set; } = "Password123!";
        public string Domain { get; set; } = "Flyhigh.local";
    }

    public partial class ActiveDirectoryService
    {
        private ADConfig _config;

        // Constructor til dit test-program (uden IConfiguration)
        public ActiveDirectoryService()
        {
            _config = new ADConfig(); // Bruger hardcoded standardværdier
        }

        // Constructor til din API (med IConfiguration for appsettings.json)
        public ActiveDirectoryService(IConfiguration configuration)
        {
            // Rettet kode
            _config = configuration.GetSection("ADConfig").Get<ADConfig>() ?? new ADConfig();
        }

        // Properties som dit test-program bruger
        public string Server => _config.Server;
        public string Username => _config.Username;
        public string Domain => _config.Domain;

        // Metode til at opdatere konfigurationen fra dit test-program
        public void UpdateConfig(string? server = null, string? username = null, string? password = null, string? domain = null)
        {
            if (!string.IsNullOrEmpty(server)) _config.Server = server;
            if (!string.IsNullOrEmpty(username)) _config.Username = username;
            if (!string.IsNullOrEmpty(password)) _config.Password = password;
            if (!string.IsNullOrEmpty(domain)) _config.Domain = domain;
        }

        public bool ValidateUserCredentials(string username, string password)
        {
            // Vi tilføjer detaljeret logging for at se den præcise fejl.
            Console.WriteLine($"--- DEBUG: Validerer bruger '{username}' ---");
            try
            {
                // Vi bruger en mere specifik PrincipalContext for at undgå problemer,
                // hvis maskinen ikke er på domænet.
                using var context = new PrincipalContext(
                    ContextType.Domain,
                    _config.Server, // Servernavn eller IP
                    null, // Container, null er fint til at starte med
                    ContextOptions.Negotiate,
                    _config.Username, // Service-konto brugernavn
                    _config.Password  // Service-konto password
                );

                Console.WriteLine($"--- DEBUG: PrincipalContext oprettet til server '{context.ConnectedServer}'. Validerer nu... ---");

                bool isValid = context.ValidateCredentials(username, password, ContextOptions.Negotiate);

                Console.WriteLine($"--- DEBUG: Validering for '{username}' returnerede: {isValid} ---");
                return isValid;
            }
            catch (Exception ex)
            {
                // LOG DEN FULDE FEJL! Dette er det vigtigste skridt.
                Console.WriteLine($"[FATAL AD ERROR] ValidateUserCredentials fejlede: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Hvis der er en indre exception, så log den også.
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"--- Inner Exception ---");
                    Console.WriteLine($"Message: {ex.InnerException.Message}");
                    Console.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
                }

                return false;
            }
        }

        public ADUser? GetUserWithGroups(string username)
        {
            try
            {
                using var context = new PrincipalContext(
                    ContextType.Domain,
                    _config.Server,
                    null,
                    _config.Username,
                    _config.Password
                );

                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

                if (userPrincipal == null)
                {
                    return null;
                }

                // --- RETTELSEN ER HER ---
                // Vi bruger nu vores hjælpe-metode til at sikre, at ALLE data (inkl. telefon) kommer med.
                var adUser = MapUserPrincipalToADUser(userPrincipal);

                // Hent brugerens grupper og tilføj dem til objektet
                var groups = userPrincipal.GetAuthorizationGroups();
                foreach (var group in groups)
                {
                    adUser.Groups.Add(group.Name);
                }

                return adUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] i GetUserWithGroups: {ex.Message}");
                return null;
            }
        }
        public void TestConnection()
        {
            Console.WriteLine("=== Tester Forbindelse ===");
            Console.WriteLine($"Forsøger at forbinde til '{_config.Server}' som '{_config.Username}'...");
            try
            {
                // Vi tester ved at oprette en PrincipalContext.
                // Hvis dette ikke kaster en exception, er server og credentials korrekte.
                using var context = new PrincipalContext(
                    ContextType.Domain,
                    _config.Server,
                    null,
                    _config.Username,
                    _config.Password
                );

                // Vi validerer, at vi rent faktisk kan tale med contexten.
                context.ValidateCredentials(_config.Username, _config.Password);

                Console.WriteLine("✅ Forbindelse succesfuld!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Forbindelse fejlede!");
                Console.WriteLine($"   Fejl: {ex.Message}");
            }
        }

    }
}