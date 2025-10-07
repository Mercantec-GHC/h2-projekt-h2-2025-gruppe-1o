# H2 Hotel Booking System - 2025

[![.NET](https://img.shields.io/badge/.NET-8-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![C#](https://img.shields.io/badge/C%23-12-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Blazor](https://img.shields.io/badge/Blazor-WASM-blue)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-darkblue)](https://www.postgresql.org/)
[![Active Directory](https://img.shields.io/badge/Active_Directory-orange)](https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/get-started/virtual-dc/active-directory-domain-services-overview)
[![SignalR](https://img.shields.io/badge/SignalR-real--time-brightgreen)](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr)

Et komplet hotelbookingsystem bygget som en del af H2-forløbet. Løsningen består af en .NET Web API backend, en Blazor WebAssembly frontend og integration med Active Directory for personale-login.

## 🧭 Projekt Oversigt

Dette projekt er et fuldt funktionelt bookingsystem til et hotel, der håndterer alt fra gæstebookinger og værelsesadministration til personalehåndtering og real-time support. Systemet er bygget med en moderne 3-lagsarkitektur, hvor en RESTful API fungerer som backend for en Single Page Application (SPA) bygget i Blazor.

## 🧩 Features

Projektet implementerer en række funktioner, opdelt efter brugerroller for at sikre en struktureret og sikker brugeroplevelse.

### For Gæster (Kunder)
- **Søg og Find:** Se en oversigt over værelsestyper og søg efter ledige værelser i en given periode for et bestemt antal gæster.
- **Booking:** Gennemfør en booking af et ledigt værelse, inklusiv tilvalg af ekstra services (f.eks. morgenmad, spa).
- **Selvbetjening:** Opret og log ind på en personlig konto for at se og administrere egne bookinger.
- **Support:** Opret og følg status på support-sager via et real-time ticket-system.

### For Personale (Receptionist, Manager)
- **Dashboard:** Få et hurtigt overblik over dagens ankomster, afrejser og hotellets nuværende belægningsprocent.
- **Booking Management:** Håndter alle bookinger, inklusiv check-in og check-ud af gæster.
- **Walk-in Booking:** Opret nye bookinger direkte i systemet for gæster uden forudgående reservation.
- **Værelsesstatus:** Se en live-oversigt over alle værelsers status (rengjort, optaget, trænger til rengøring).
- **Support Håndtering:** Se og besvar indkomne support-sager fra gæster via et real-time interface.

### For Rengøringspersonale
- **Opgaveliste:** Se en specifik liste over værelser, der kræver rengøring.
- **Status Opdatering:** Markér et værelse som "Rengjort", hvilket automatisk opdaterer systemet i realtid.
- **Rapportering:** Opret en support-sag direkte fra dashboardet, hvis der opdages et problem på et værelse (f.eks. en defekt lampe).

### For Manager/Admin
- **Fuld adgang:** Adgang til alle receptionist-funktioner.
- **Statistik:** Se udvidet statistik over hotellets performance, såsom daglig omsætning.
- **Active Directory Management:** Administrer personale-brugere og grupper direkte fra et dedikeret AD-dashboard i frontend.

## ⚙️ Teknologi-stak

Løsningen er bygget med følgende teknologier:

- **Backend (API):**
  - C# 12 og .NET 8
  - ASP.NET Core Web API
  - Entity Framework Core 8
  - JWT (JSON Web Tokens) for sikkerhed
  - SignalR for real-time kommunikation
  - BCrypt.NET til hashing af passwords
  - Serilog til struktureret logging

- **Frontend (Client):**
  - Blazor WebAssembly
  - .NET 8 / C#
  - HTML/CSS

- **Database:**
  - PostgreSQL (hostet på Neon.tech)

- **Autentificering:**
  - **Gæster:** Lokal database med email/password.
  - **Personale:** Active Directory (via LDAP-integration).
  - **Test-Personale:** Lokal databse med email/password

- **Deployment:**
  - Docker & Dokploy (hostet på `deploy.mercantec.tech`).

## 🚀 Getting Started

Følg disse trin for at opsætte og køre projektet lokalt.

### 1. Forudsætninger
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- En IDE som [Visual Studio 2022](https://visualstudio.microsoft.com/) eller [JetBrains Rider](https://www.jetbrains.com/rider/).
- En PostgreSQL-klient som [DBeaver](https://dbeaver.io/) eller [TablePlus](https://tableplus.com/) (valgfrit).

### 2. Installation
1. Klon dette repository til din lokale maskine:
   ```bash
   git clone <repository-url>
   ```
2. Åbn løsningen (`.sln`-fil) i din foretrukne IDE.

### 3. Konfiguration
For at API'en kan køre korrekt, skal du oprette en `appsettings.Development.json`-fil i `API`-projektets rodmappe. Kopier indholdet fra `appsettings.json` og udfyld de nødvendige værdier:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Din_PostgreSQL_Connection_String_fra_Neon.tech"
  },
  "SendGridSettings": {
    "ApiKey": "Din_SendGrid_API_Nøgle",
    "FromEmail": "noreply@flyhigh.dk",
    "FromName": "Flyhigh Hotel System"
  },
  "Jwt": {
    "SecretKey": "EN_MEGET_LANG_OG_SIKKER_HEMMELIGHED_HER_MINIMUM_32_TEGN",
    "Issuer": "FLYHIGHHOTEL-API-LOCAL",
    "Audience": "FLYHIGHHOTEL-Client-LOCAL",
    "ExpiryMinutes": "60"
  },
  "ADConfig": {
    "Server": "IP_Til_Din_AD_Server",
    "Username": "Admin_Bruger_Til_AD",
    "Password": "Password_Til_Admin_Bruger",
    "Domain": "Dit_AD_Domæne.local"
  },
  // ... resten af indstillingerne
}
```

- **`ConnectionStrings`**: Forbindelsesstrengen til din PostgreSQL-database (f.eks. fra Neon.tech).
- **`SendGridSettings`**: Nødvendig for at kunne sende e-mails (f.eks. bookingbekræftelser). Du kan oprette en gratis konto på [SendGrid](https://sendgrid.com/).
- **`Jwt:SecretKey`**: En lang, tilfældig og hemmelig streng, som bruges til at signere JWT-tokens.
- **`ADConfig`**: Oplysninger for at forbinde til din lokale Active Directory-server.

### 4. Database Setup
Kør Entity Framework Core-migrationer for at oprette databasestrukturen. Åbn "Package Manager Console" i Visual Studio og kør:
```powershell
Update-Database
```
Dette vil oprette alle tabeller og seede de statiske data (roller, værelsestyper etc.) i din database.

### 5. Kør applikationen
Konfigurer din IDE til at starte **både** `API`- og `Blazor`-projekterne samtidigt.
- I Visual Studio: Højreklik på Solution -> `Set Startup Projects...` -> Vælg `Multiple startup projects` og sæt `Action` til `Start` for både `API` og `Blazor`.

- **API'en** vil typisk køre på `https://localhost:8091`.
- **Blazor-klienten** vil typisk køre på `https://localhost:7285`.

## 🏗️ Projektstruktur

Løsningen er opdelt i fire hovedprojekter for at sikre en ren og skalerbar arkitektur:

- **`API`**: ASP.NET Core Web API-projektet. Håndterer al forretningslogik, databaseadgang, autentificering og fungerer som backend for systemet.
- **`Blazor`**: Blazor WebAssembly (WASM) projektet. Dette er vores frontend (klient-side), som brugerne interagerer med.
- **`DomainModels`**: Et .NET class library, der indeholder alle fælles datamodeller, DTO'er og enums. Dette projekt deles mellem `API` og `Blazor` for at sikre konsistens.
- **`ActiveDirectoryTesting`**: Et bibliotek, der indeholder `ActiveDirectoryService`. Denne service abstraherer al kommunikation med Active Directory (LDAP) væk fra API-controllerne.

## 📦 Deployment

Projektet er konfigureret til deployment via Docker. Hvert projekt (`API` og `Blazor`) indeholder en `Dockerfile`, som bygger et produktionsklart image. Disse images deployes til en Dokploy-instans, der kører på `deploy.mercantec.tech`.

## 🙏 Kreditering

Dette projekt er udviklet som en del af H2-forløbet på Mercantec under vejledning af Mathias G. Sørensen (MAGS).