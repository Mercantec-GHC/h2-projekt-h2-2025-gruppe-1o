using System.DirectoryServices.AccountManagement;

namespace ActiveDirectoryTesting
{
    public partial class ActiveDirectoryService
    {
        /// <summary>
        /// Avanceret søgning efter brugere med flere kriterier.
        /// </summary>
        public List<ADUser> AdvancedUserSearch(string? name = null, string? email = null, string? department = null, string? title = null)
        {
            var users = new List<ADUser>();
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);

                // Opret "query by example" bruger med de angivne kriterier
                var userExample = new UserPrincipal(context);
                if (!string.IsNullOrWhiteSpace(name)) userExample.DisplayName = $"*{name}*";
                if (!string.IsNullOrWhiteSpace(email)) userExample.EmailAddress = $"*{email}*";
                // Department og Title er ikke standard-properties, så de kan ikke bruges direkte her.
                // Simpel søgning dækker de mest almindelige scenarier.

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
                Console.WriteLine($"[FEJL] Fejl i avanceret brugersøgning: {ex.Message}");
            }
            return users;
        }

        /// <summary>
        /// Avanceret søgning efter grupper med flere kriterier.
        /// </summary>
        public List<ADGroup> AdvancedGroupSearch(string? name = null, string? description = null, bool? hasMembers = null)
        {
            var groups = new List<ADGroup>();
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                var groupExample = new GroupPrincipal(context);
                if (!string.IsNullOrWhiteSpace(name)) groupExample.Name = $"*{name}*";
                if (!string.IsNullOrWhiteSpace(description)) groupExample.Description = $"*{description}*";

                using var searcher = new PrincipalSearcher(groupExample);
                foreach (var result in searcher.FindAll())
                {
                    if (result is GroupPrincipal groupPrincipal)
                    {
                        var members = groupPrincipal.GetMembers();

                        // Filtrer på 'hasMembers' efter søgningen
                        if (hasMembers.HasValue)
                        {
                            if (hasMembers.Value && !members.Any()) continue; // Spring over hvis gruppen skal have medlemmer, men ikke har det
                            if (!hasMembers.Value && members.Any()) continue; // Spring over hvis gruppen IKKE skal have medlemmer, men har det
                        }

                        groups.Add(new ADGroup
                        {
                            Name = groupPrincipal.Name,
                            Description = groupPrincipal.Description ?? "N/A",
                            Members = members.Select(m => m.SamAccountName).ToList()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Fejl i avanceret gruppesøgning: {ex.Message}");
            }
            return groups;
        }

        /// <summary>
        /// Søger efter brugere i en specifik gruppe.
        /// </summary>
        public List<ADUser> GetUsersInGroup(string groupName)
        {
            var users = new List<ADUser>();
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                var group = GroupPrincipal.FindByIdentity(context, groupName);

                if (group != null)
                {
                    foreach (var member in group.GetMembers(true)) // true = recursive
                    {
                        if (member is UserPrincipal userPrincipal)
                        {
                            users.Add(MapUserPrincipalToADUser(userPrincipal));
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Gruppen '{groupName}' blev ikke fundet.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Kunne ikke hente brugere fra gruppen '{groupName}': {ex.Message}");
            }
            return users;
        }

        // UI-metoderne nedenfor er primært til konsol-app'en og behøver ikke ændres,
        // da de kalder de metoder, vi lige har rettet ovenfor.
        public void ShowAdvancedSearchMenu() { /* ... Din eksisterende kode her ... */ }
        private void ShowAdvancedUserSearch() { /* ... Din eksisterende kode her ... */ }
        private void ShowAdvancedGroupSearch() { /* ... Din eksisterende kode her ... */ }
        private void ShowUsersInGroup() { /* ... Din eksisterende kode her ... */ }
    }
}