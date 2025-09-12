using System.DirectoryServices.AccountManagement;

namespace ActiveDirectoryTesting
{
    public partial class ActiveDirectoryService
    {
        /// <summary>
        /// Henter alle brugere fra Active Directory.
        /// </summary>
        public List<ADUser> GetAllUsers()
        {
            var users = new List<ADUser>();
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                using var searcher = new PrincipalSearcher(new UserPrincipal(context));

                foreach (var result in searcher.FindAll())
                {
                    if (result is UserPrincipal userPrincipal)
                    {
                        users.Add(MapUserPrincipalToADUser(userPrincipal));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Kunne ikke hente alle brugere: {ex.Message}");
            }
            return users;
        }

        /// <summary>
        /// Søger efter brugere baseret på en generel søgeterm.
        /// </summary>
        public List<ADUser> SearchUsers(string searchTerm)
        {
            var users = new List<ADUser>();
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);

                // Opret en "query by example" bruger
                var userExample = new UserPrincipal(context)
                {
                    // Søg i flere felter med wildcards
                    DisplayName = $"*{searchTerm}*",
                    SamAccountName = $"*{searchTerm}*",
                    EmailAddress = $"*{searchTerm}*"
                };

                using var searcher = new PrincipalSearcher(userExample);

                foreach (var result in searcher.FindAll())
                {
                    if (result is UserPrincipal userPrincipal)
                    {
                        users.Add(MapUserPrincipalToADUser(userPrincipal));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Fejl under brugersøgning: {ex.Message}");
            }
            return users;
        }

        /// <summary>
        /// Henter alle grupper for en specifik bruger. Meget simplere med den nye metode!
        /// </summary>
        public List<string> GetUserGroups(string username)
        {
            var groupNames = new List<string>();
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

                if (userPrincipal != null)
                {
                    // Denne metode håndterer også nested groups
                    var groups = userPrincipal.GetAuthorizationGroups();
                    foreach (var group in groups)
                    {
                        groupNames.Add(group.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Kunne ikke hente grupper for bruger '{username}': {ex.Message}");
            }
            return groupNames;
        }

        /// <summary>
        /// Viser alle brugere i konsollen. (Ingen logikændring her, den kalder bare den rettede metode).
        /// </summary>
        public void ShowAllUsers()
        {
            Console.WriteLine("=== Alle Brugere ===");
            var users = GetAllUsers();

            if (!users.Any())
            {
                Console.WriteLine("Ingen brugere fundet.");
                return;
            }

            Console.WriteLine($"\nFundet {users.Count} brugere:");
            foreach (var user in users.OrderBy(u => u.Name))
            {
                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Navn:       {user.Name}");
                Console.WriteLine($"Brugernavn: {user.Username}");
                Console.WriteLine($"Email:      {user.Email}");
            }
        }

        /// <summary>
        /// UI-metode til at søge og vise brugere.
        /// </summary>
        public void SearchUsers()
        {
            Console.Write("Indtast søgeterm (navn, brugernavn, email): ");
            var searchTerm = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Console.WriteLine("Søgeterm kan ikke være tom.");
                return;
            }

            var users = SearchUsers(searchTerm);
            if (!users.Any())
            {
                Console.WriteLine("Ingen brugere matchede søgningen.");
                return;
            }

            Console.WriteLine($"\nFundet {users.Count} brugere:");
            foreach (var user in users.OrderBy(u => u.Name))
            {
                Console.WriteLine(new string('-', 50));
                ShowUserDetails(user);
            }
        }

        /// <summary>
        /// Viser detaljerede oplysninger for en given ADUser.
        /// </summary>
        public void ShowUserDetails(ADUser user)
        {
            Console.WriteLine($"👤 Navn:       {user.DisplayName} ({user.Username})");
            Console.WriteLine($"📧 Email:      {user.Email}");
            Console.WriteLine($"🏢 Afdeling:   {user.Department}");
            Console.WriteLine($"👔 Titel:      {user.Title}");
            Console.WriteLine($"🔐 Status:     {(user.IsEnabled ? "Aktiv" : "Deaktiveret")}");
        }

        /// <summary>
        /// UI-metode til at vise den aktuelle brugers info.
        /// </summary>
        public void ShowCurrentUserInfo()
        {
            Console.WriteLine("=== Min Bruger Information (/me) ===");
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, _config.Username);

                if (userPrincipal == null)
                {
                    Console.WriteLine("Kunne ikke finde den aktuelle bruger.");
                    return;
                }

                var adUser = MapUserPrincipalToADUser(userPrincipal);
                adUser.Groups = GetUserGroups(adUser.Username); // Hent grupper

                ShowUserDetails(adUser); // Genbrug detaljevisning

                Console.WriteLine($"\n👥 Grupper/Roller ({adUser.Groups.Count}):");
                adUser.Groups.OrderBy(g => g).ToList().ForEach(g => Console.WriteLine($"   - {g}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Kunne ikke hente nuværende brugerinfo: {ex.Message}");
            }
        }

        /// <summary>
        /// En hjælpe-metode til at konvertere et UserPrincipal-objekt til vores egen ADUser-klasse.
        /// </summary>
        private ADUser MapUserPrincipalToADUser(UserPrincipal user)
        {
            // Hent både mobil og telefon. Prioriter mobilnummeret.
            var mobilePhone = user.GetAttributeValue("mobile");
            var mainPhone = user.VoiceTelephoneNumber;

            return new ADUser
            {
                Name = user.Name,
                Username = user.SamAccountName,
                Email = user.EmailAddress ?? "N/A",

                // Ny logik: Brug mobil, hvis det findes, ellers brug almindelig telefon.
                Phone = !string.IsNullOrWhiteSpace(mobilePhone) ? mobilePhone : (mainPhone ?? string.Empty),

                DistinguishedName = user.DistinguishedName,
                FirstName = user.GivenName,
                LastName = user.Surname,
                DisplayName = user.DisplayName,
                LastLogon = user.LastLogon,
                IsEnabled = user.Enabled ?? false
            };
        }
    }

    // Nødvendig hjælpe-klasse for at kunne hente 'mobile'-attributten
    public static class PrincipalExtensions
    {
        public static string? GetAttributeValue(this Principal principal, string attribute)
        {
            var directoryEntry = principal.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;
            if (directoryEntry != null && directoryEntry.Properties.Contains(attribute))
            {
                return directoryEntry.Properties[attribute].Value?.ToString();
            }
            return null;
        }
    }
}