using Microsoft.Extensions.Configuration;
using System.DirectoryServices.AccountManagement;

namespace ActiveDirectoryTesting
{
    // Komplette DTO-klasser
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

    /// <summary>
    /// En samlet service til al interaktion med Active Directory.
    /// </summary>
    public class ActiveDirectoryService
    {
        private readonly ADConfig _config;

        public ActiveDirectoryService() { _config = new ADConfig(); }
        public ActiveDirectoryService(IConfiguration configuration) { _config = configuration.GetSection("ADConfig").Get<ADConfig>() ?? new ADConfig(); }

        public string Server => _config.Server;
        public string Username => _config.Username;
        public string Domain => _config.Domain;
        public void UpdateConfig(string? server = null, string? username = null, string? password = null, string? domain = null)
        {
            if (!string.IsNullOrEmpty(server)) _config.Server = server;
            if (!string.IsNullOrEmpty(username)) _config.Username = username;
            if (!string.IsNullOrEmpty(password)) _config.Password = password;
            if (!string.IsNullOrEmpty(domain)) _config.Domain = domain;
        }

        private PrincipalContext GetPrincipalContext() => new PrincipalContext(ContextType.Domain, _config.Server, null, ContextOptions.Negotiate, _config.Username, _config.Password);

        public bool ValidateUserCredentials(string username, string password) { try { using var c = GetPrincipalContext(); return c.ValidateCredentials(username, password, ContextOptions.Negotiate); } catch (Exception ex) { Console.WriteLine($"[FATAL AD ERROR] ValidateUserCredentials fejlede: {ex.Message}"); return false; } }

        public List<ADUser> GetAllUsers()
        {
            var list = new List<ADUser>();
            try { using var c = GetPrincipalContext(); using var s = new PrincipalSearcher(new UserPrincipal(c)); foreach (var r in s.FindAll()) { if (r is UserPrincipal u) list.Add(MapUserPrincipalToADUser(u)); } } catch (Exception e) { Console.WriteLine($"[FEJL] Kunne ikke hente alle brugere: {e.Message}"); }
            return list;
        }

        public ADUser? GetUserWithGroups(string username)
        {
            try
            {
                using var context = GetPrincipalContext();
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                if (userPrincipal == null) return null;
                var adUser = MapUserPrincipalToADUser(userPrincipal);
                adUser.Groups = userPrincipal.GetAuthorizationGroups().Select(g => g.Name).ToList();
                return adUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] i GetUserWithGroups: {ex.Message}");
                return null;
            }
        }

        // --- START RETTELSE: Parameternavne er gendannet ---
        public List<ADGroup> AdvancedGroupSearch(string? name = null, string? description = null, bool? hasMembers = null)
        {
            var list = new List<ADGroup>();
            try
            {
                using var c = GetPrincipalContext(); var groupExample = new GroupPrincipal(c);
                if (!string.IsNullOrWhiteSpace(name)) groupExample.Name = $"*{name}*";
                if (!string.IsNullOrWhiteSpace(description)) groupExample.Description = $"*{description}*";
                using var s = new PrincipalSearcher(groupExample);
                foreach (var r in s.FindAll())
                {
                    if (r is GroupPrincipal gp)
                    {
                        var members = gp.GetMembers().ToList();
                        if (hasMembers.HasValue && ((hasMembers.Value && !members.Any()) || (!hasMembers.Value && members.Any()))) continue;
                        list.Add(new ADGroup { Name = gp.Name, Description = gp.Description ?? "", Members = members.Select(m => m.SamAccountName).ToList()! });
                    }
                }
            }
            catch (Exception e) { Console.WriteLine($"[FEJL] Fejl i avanceret gruppesøgning: {e.Message}"); }
            return list;
        }
        // --- SLUT RETTELSE ---

        private ADUser MapUserPrincipalToADUser(UserPrincipal u)
        {
            var mob = u.GetAttributeValue("mobile"); var tel = u.VoiceTelephoneNumber;
            return new ADUser { Name = u.Name, Username = u.SamAccountName, Email = u.EmailAddress ?? "N/A", Phone = !string.IsNullOrWhiteSpace(mob) ? mob : (tel ?? ""), DistinguishedName = u.DistinguishedName, FirstName = u.GivenName, LastName = u.Surname, DisplayName = u.DisplayName, LastLogon = u.LastLogon, IsEnabled = u.Enabled ?? false };
        }

        public (bool Success, string Message) CreateUser(string username, string password, string firstName, string lastName, string email)
        {
            try
            {
                using var context = GetPrincipalContext();
                if (UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username) != null) return (false, "En bruger med dette brugernavn eksisterer allerede.");
                using var user = new UserPrincipal(context) { SamAccountName = username, GivenName = firstName, Surname = lastName, EmailAddress = email, DisplayName = $"{firstName} {lastName}", Enabled = true, PasswordNeverExpires = false };
                user.SetPassword(password); user.Save();
                return (true, "Brugeren blev oprettet succesfuldt.");
            }
            catch (Exception ex) { return (false, $"Fejl fra Active Directory: {ex.Message}"); }
        }

        public (bool Success, string Message) ResetUserPassword(string username, string newPassword) { try { using var c = GetPrincipalContext(); var u = UserPrincipal.FindByIdentity(c, IdentityType.SamAccountName, username); if (u == null) return (false, "Bruger ikke fundet."); u.SetPassword(newPassword); u.Save(); return (true, "Adgangskode er nulstillet."); } catch (Exception ex) { return (false, $"Fejl fra Active Directory: {ex.Message}"); } }
        public (bool Success, string Message) SetUserStatus(string username, bool isEnabled) { try { using var c = GetPrincipalContext(); var u = UserPrincipal.FindByIdentity(c, IdentityType.SamAccountName, username); if (u == null) return (false, "Bruger ikke fundet."); u.Enabled = isEnabled; u.Save(); return (true, "Brugerstatus er opdateret."); } catch (Exception ex) { return (false, $"Fejl fra Active Directory: {ex.Message}"); } }
        public (bool Success, string Message) AddUserToGroup(string username, string groupName) { try { using var c = GetPrincipalContext(); var u = UserPrincipal.FindByIdentity(c, IdentityType.SamAccountName, username); var g = GroupPrincipal.FindByIdentity(c, IdentityType.Name, groupName); if (u == null || g == null) return (false, "Bruger eller gruppe blev ikke fundet."); if (!g.Members.Contains(u)) { g.Members.Add(u); g.Save(); } return (true, "Bruger tilføjet til gruppe."); } catch (Exception ex) { return (false, $"Fejl fra Active Directory: {ex.Message}"); } }
        public (bool Success, string Message) RemoveUserFromGroup(string username, string groupName) { try { using var c = GetPrincipalContext(); var u = UserPrincipal.FindByIdentity(c, IdentityType.SamAccountName, username); var g = GroupPrincipal.FindByIdentity(c, IdentityType.Name, groupName); if (u == null || g == null) return (false, "Bruger eller gruppe blev ikke fundet."); if (g.Members.Contains(u)) { g.Members.Remove(u); g.Save(); } return (true, "Bruger fjernet fra gruppe."); } catch (Exception ex) { return (false, $"Fejl fra Active Directory: {ex.Message}"); } }

        // Metoder KUN til konsol-app'en
        public void TestConnection() { try { using var c = GetPrincipalContext(); c.ValidateCredentials(_config.Username, _config.Password); Console.WriteLine("✅ OK"); } catch (Exception e) { Console.WriteLine($"❌ Fejl: {e.Message}"); } }
        public void ShowAllUsers() => GetAllUsers().OrderBy(u => u.Name).ToList().ForEach(u => Console.WriteLine($"- {u.DisplayName} ({u.Username}) | {u.Email}"));
        public void ShowAllGroups() => AdvancedGroupSearch().OrderBy(g => g.Name).ToList().ForEach(g => Console.WriteLine($"- {g.Name}"));
        public void ShowGroupsWithMembers() => AdvancedGroupSearch().ForEach(g => { Console.WriteLine($"\n--- Gruppe: {g.Name} ---"); if (g.Members.Any()) g.Members.ForEach(m => Console.WriteLine($"  - {m}")); else Console.WriteLine("  (Ingen medlemmer)"); });
        public void ShowCurrentUserInfo()
        {
            var user = GetUserWithGroups(_config.Username);
            if (user == null) return;
            Console.WriteLine($"Bruger: {user.DisplayName}");
            Console.WriteLine("Grupper:");
            user.Groups.ForEach(g => Console.WriteLine($" - {g}"));
        }
        public void SearchGroups() { Console.Write("Søgeterm: "); AdvancedGroupSearch(Console.ReadLine()).ForEach(g => Console.WriteLine($"- {g.Name}")); }
        public void SearchUsers() { Console.Write("Søgeterm: "); /* Ikke implementeret i menu */ }
        public void ShowAdvancedSearchMenu() { Console.WriteLine("Ikke implementeret i menu."); }
    }

    public static class PrincipalExtensions
    {
        public static string? GetAttributeValue(this Principal principal, string attribute)
        {
            var de = principal.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;
            return de?.Properties.Contains(attribute) == true ? de.Properties[attribute].Value?.ToString() : null;
        }
    }
}