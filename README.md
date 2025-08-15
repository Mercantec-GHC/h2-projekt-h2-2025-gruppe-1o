# H2-Projekt Gruppe 1o

Projektet han findes her - [H2 Projekt forløb på Notion](https://mercantec.notion.site/h2f)

Det er delt op i 4 mapper (3 hovedprojekter og Aspire)

## [Blazor](/Blazor/)

Vi anbefaler at I bruger Blazor WebAssembly, da det er det vi underviser i. Den er koblet op på vores API gemmen APIService klassen i Blazor.

## [Domain Models](/DomainModels/)

Her er alle jeres klasser, som I skal bruge til jeres Blazor og API.
Domain Models / Class Libary versionen er nu opdateret til .NET 9.0

## [API](/API/)

# H2 Hotel Booking System - API

Dette er backend-API'et for H2 Hotel Booking System, bygget med .NET 9, C#, og Entity Framework Core. API'et håndterer brugerautentifikation, bookinger og værelsesadministration.

## Kernefunktionaliteter (Release 1)

* **Brugerhåndtering:** Sikker registrering og login med BCrypt password hashing.
* **Database:** Forbundet til en PostgreSQL database via Entity Framework Core.
* **Sikkerhed:** JWT-baseret autentifikation til at beskytte endpoints.
* **Grundlæggende API:** Endpoints til at oprette, læse, opdatere og slette brugere.

## Kom Godt I Gang

1.  **Klon repository'et:**
    `git clone <din-repo-url>`
2.  **Konfigurer forbindelsen:**
    Opdater `ConnectionStrings` i `API/appsettings.json` til din PostgreSQL database.
3.  **Kør database migration:**
    `dotnet ef database update`
4.  **Kør projektet:**
    `dotnet run --project API`

## [Aspire](/H2-Projekt.AppHost/)

Aspire er vores hosting platform, den er koblet op til vores API og Blazor. Det er ikke obligatorisk at bruge Aspire, men det anbefales. Vi bruger Aspire med .NET 9.0

### Hosting

Vi udforsker forskellige hosting muligheder på H2 - men vil helst vores lokale datacenter. På H2 bruger vi Windows Server 2022 som platform - det introducerede vi senere i forløbet.
