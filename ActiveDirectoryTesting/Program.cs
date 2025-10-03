namespace ActiveDirectoryTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var adTester = new ADTester();
            adTester.Run();
        }
    }

    public class ADTester
    {
        // Denne linje vil nu virke, fordi vi har tilføjet en tom constructor
        private ActiveDirectoryService _adService = new ActiveDirectoryService();

        public void Run()
        {
            Console.WriteLine("=== Active Directory Tester ===");
            while (true)
            {
                ShowMenu();
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": _adService.ShowAllGroups(); break;
                        case "2": _adService.ShowAllUsers(); break;
                        case "3": _adService.ShowGroupsWithMembers(); break;
                        case "4": _adService.SearchUsers(); break;
                        case "5": _adService.SearchGroups(); break;
                        case "6": UpdateConfig(); break;
                        case "8": _adService.ShowAdvancedSearchMenu(); break;
                        case "0": Console.WriteLine("Farvel!"); return;
                        default: Console.WriteLine("Ugyldigt valg. Prøv igen."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fejl: {ex.Message}");
                }

                Console.WriteLine("\nTryk på en tast for at fortsætte...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("=== Hovedmenu ===");
            // Disse linjer vil nu virke, fordi vi har tilføjet de manglende properties
            Console.WriteLine($"Server: {_adService.Server}");
            Console.WriteLine($"Bruger: {_adService.Username}@{_adService.Domain}");
            Console.WriteLine();
            Console.WriteLine("1. Vis alle grupper");
            Console.WriteLine("2. Vis alle brugere");
            Console.WriteLine("3. Vis grupper med medlemmer");
            Console.WriteLine("4. Søg efter brugere");
            Console.WriteLine("5. Søg efter grupper");
            Console.WriteLine("6. Opdater forbindelsesindstillinger");
            Console.WriteLine("7. Test forbindelse");
            Console.WriteLine("8. Avanceret søgning");
            Console.WriteLine("9. /me - Vis min bruger information");
            Console.WriteLine("0. Afslut");
            Console.WriteLine();
            Console.Write("Vælg en option: ");
        }

        private void UpdateConfig()
        {
            Console.WriteLine("=== Opdater forbindelsesindstillinger ===");
            Console.WriteLine("Tryk Enter for at beholde nuværende værdi\n");

            Console.Write($"Server ({_adService.Server}): ");
            var server = Console.ReadLine();

            Console.Write($"Brugernavn ({_adService.Username}): ");
            var username = Console.ReadLine();

            Console.Write("Adgangskode: ");
            var password = Console.ReadLine();

            Console.Write($"Domæne ({_adService.Domain}): ");
            var domain = Console.ReadLine();

            _adService.UpdateConfig(server, username, password, domain);
            Console.WriteLine("Indstillinger opdateret!");
        }
    }
}