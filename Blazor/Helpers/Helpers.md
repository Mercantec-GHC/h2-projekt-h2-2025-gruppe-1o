Formålet med Helpers i et Blazor-projekt
Helpers-mappen bruges til hjælpeklasser og metoder, der understøtter resten af applikationen med små, genanvendelige funktioner. Helpers indeholder typisk kode, der ikke passer ind i services eller komponenter, men som stadig er nyttig flere steder, f.eks. formateringsfunktioner, konverteringer eller simple beregninger.
Eksempler på helpers:
Dato- og tidsformatering.
Konvertering af data fra ét format til et andet.
Små utility-metoder, der letter arbejdet i komponenter eller services.
Helpers er med til at holde koden DRY (Don't Repeat Yourself) og overskuelig.

Forskellen på Helpers og Services
Helpers bør bruges til:

- Små hjælpefunktioner uden afhængigheder eller konfiguration.
- Utility-metoder til f.eks. formatering, konvertering eller simple beregninger.
- Funktioner, der ikke skal injiceres, men blot kaldes direkte.

Services bør bruges til:

- Forretningslogik, datahentning eller kommunikation med eksterne systemer (f.eks. API-kald).
- Funktionalitet, der skal genbruges på tværs af komponenter og ofte kræver afhængigheder.
- Logik, der kan have brug for at blive testet isoleret fra UI.

Kort sagt: Helpers er til små, simple funktioner, mens services håndterer større logik og integration.
