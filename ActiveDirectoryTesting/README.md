# Active Directory Tester

En C# konsol-applikation til at teste forbindelse til Active Directory via LDAP og udføre forskellige operationer på brugere og grupper.

## Arkitektur

Projektet er opdelt i en partiel klasse `ActiveDirectoryService` med separate filer for hver større funktion:

- **ActiveDirectoryService.cs** - Hovedfil med modeller og grundlæggende struktur
- **ActiveDirectoryService.Connection.cs** - Forbindelseslogik og test funktioner
- **ActiveDirectoryService.Groups.cs** - Gruppeoperationer og visning
- **ActiveDirectoryService.Users.cs** - Brugeroperationer og visning  
- **ActiveDirectoryService.Search.cs** - Avancerede søgefunktioner
- **Program.cs** - Hovedprogram og menu system

## Funktioner

- **Få en liste af alle grupper** - Henter alle grupper fra AD med navn og beskrivelse
- **Få en liste af alle brugere** - Henter alle brugere med detaljerede oplysninger
- **Få en kombineret liste af alle grupper og deres medlemmer** - Viser grupper med deres medlemmer
- **Søg efter specifikke brugere eller grupper** - Søg baseret på navn, email, afdeling, titel eller beskrivelse
- **Avanceret søgning** - Multi-kriterie søgning med flere filtre
- **Find brugere i specifik gruppe** - Viser alle brugere i en bestemt gruppe
- **Runtime konfiguration** - Mulighed for at ændre forbindelsesindstillinger under kørsel
- **Forbindelsestest** - Test forbindelsen til AD serveren med detaljeret information

## Standard Konfiguration

Programmet kommer med følgende standard indstillinger:
- **Server**: 10.133.71.111
- **Brugernavn**: Admin
- **Adgangskode**: Cisco1234!
- **Domæne**: Hotel.local

## Kørsel

1. Kompiler programmet:
   ```bash
   dotnet build
   ```

2. Kør programmet:
   ```bash
   dotnet run
   ```

## Menu System

Programmet viser en interaktiv menu med følgende muligheder:

1. **Vis alle grupper** - Lister alle grupper i AD
2. **Vis alle brugere** - Lister alle brugere i AD
3. **Vis grupper med medlemmer** - Viser grupper med deres medlemmer
4. **Søg efter brugere** - Søg efter brugere baseret på forskellige kriterier
5. **Søg efter grupper** - Søg efter grupper baseret på navn eller beskrivelse
6. **Opdater forbindelsesindstillinger** - Ændre server, brugernavn, adgangskode eller domæne
7. **Test forbindelse** - Test forbindelsen til AD serveren
8. **Avanceret søgning** - Multi-kriterie søgning og find brugere i grupper
0. **Afslut** - Afslut programmet

## Søgefunktioner

### Brugersøgning
Du kan søge efter brugere baseret på:
- Navn (cn)
- Email adresse
- Afdeling
- Titel

### Gruppesøgning
Du kan søge efter grupper baseret på:
- Gruppens navn
- Gruppens beskrivelse

## Tekniske Detaljer

- **Framework**: .NET 9.0
- **LDAP Library**: System.DirectoryServices.Protocols
- **Autentificering**: Negotiate (Kerberos/NTLM)
- **Søgeomfang**: Hele domænet (Subtree)

## Fejlhåndtering

Programmet inkluderer omfattende fejlhåndtering og vil vise brugervenlige fejlmeddelelser hvis:
- Forbindelsen til AD serveren fejler
- Søgninger ikke returnerer resultater
- Der opstår problemer med autentificering

## Sikkerhed

- Adgangskoder vises ikke i menuen
- Forbindelser lukkes korrekt efter brug
- Fejlmeddelelser indeholder ikke følsomme oplysninger
