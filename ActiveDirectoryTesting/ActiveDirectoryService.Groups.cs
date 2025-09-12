using System.DirectoryServices.AccountManagement;

namespace ActiveDirectoryTesting
{
    public partial class ActiveDirectoryService
    {
        /// <summary>
        /// Viser alle grupper i Active Directory.
        /// </summary>
        public void ShowAllGroups()
        {
            Console.WriteLine("=== Alle Grupper ===");
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                using var searcher = new PrincipalSearcher(new GroupPrincipal(context));

                int count = 0;
                foreach (var result in searcher.FindAll())
                {
                    if (result is GroupPrincipal group)
                    {
                        Console.WriteLine($"- {group.Name} (Description: {group.Description ?? "N/A"})");
                        count++;
                    }
                }
                Console.WriteLine($"\nTotal antal grupper fundet: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Kunne ikke hente grupper: {ex.Message}");
            }
        }

        /// <summary>
        /// Viser grupper med deres medlemmer.
        /// </summary>
        public void ShowGroupsWithMembers()
        {
            Console.WriteLine("=== Grupper med Medlemmer ===");
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);
                using var searcher = new PrincipalSearcher(new GroupPrincipal(context));

                foreach (var result in searcher.FindAll())
                {
                    if (result is GroupPrincipal group)
                    {
                        Console.WriteLine($"\n--- Gruppe: {group.Name} ---");
                        try
                        {
                            var members = group.GetMembers();
                            if (members.Any())
                            {
                                foreach (Principal member in members)
                                {
                                    Console.WriteLine($"  - {member.SamAccountName} ({member.DisplayName})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("  (Ingen medlemmer i denne gruppe)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  [FEJL] Kunne ikke hente medlemmer for denne gruppe: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Kunne ikke hente grupper: {ex.Message}");
            }
        }

        // ---------- TILFØJ DENNE NYE METODE ----------
        /// <summary>
        /// UI-metode til at søge efter grupper og vise resultaterne.
        /// </summary>
        public void SearchGroups()
        {
            Console.WriteLine("=== Søg efter Grupper ===");
            Console.Write("Indtast søgeterm (navn eller beskrivelse): ");
            var searchTerm = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Console.WriteLine("Søgeterm kan ikke være tom.");
                return;
            }

            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _config.Server, null, _config.Username, _config.Password);

                // Opret en "query by example" gruppe
                var groupExample = new GroupPrincipal(context)
                {
                    Name = $"*{searchTerm}*"
                };

                using var searcher = new PrincipalSearcher(groupExample);
                var results = searcher.FindAll().ToList();

                if (!results.Any())
                {
                    Console.WriteLine("Ingen grupper matchede din søgning.");
                    return;
                }

                Console.WriteLine($"\nFundet {results.Count} grupper:");
                foreach (var result in results)
                {
                    if (result is GroupPrincipal group)
                    {
                        Console.WriteLine($"- {group.Name} (Description: {group.Description ?? "N/A"})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEJL] Fejl under gruppesøgning: {ex.Message}");
            }
        }
    }
}