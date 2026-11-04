# Musikdatabase

## Status

Koden er klar til SQL Server/Azure SQL og er publiceret. Standardvalget i filen er `List`.
Azure-databasen `MusicCollection` er oprettet på `restmusic-sofie-20261104`.
Gratis-tilbuddet er aktivt med ekstra betaling slået fra.
Musiktabellen, migrationshistorikken og backendens adgang er oprettet.
Backendens App Service har `Repository__Type = Database` og forbindelsen `MusicDatabase`.
De fire oprindelige numre er overført til databasen.

Kontrolleret 4. november 2026: login, oprettelse, opdatering og sletning gennem API'et.
Et midlertidigt nummer beholdt samme id, titel og opdaterede varighed efter genstart
af RESTmusicProject. Testnummeret er derefter slettet; de fire oprindelige numre er bevaret.
79 automatiske backendtests er bestået. Release-build: 0 fejl og 0 advarsler.

Publicering af koden opretter ikke tabeller automatisk. Migrationer og SQL-script
er med i projektet, så databasen kan genskabes til afleveringen.

## Skift repository

`Repository:Type` i appsettings.json kan være `List` eller `Database`.
På Azure bruges app-indstillingen `Repository__Type`, som overskriver filens værdi.

- `List`: Et fælles liste-repository i hukommelsen. Nulstilles ved genstart.
- `Database`: Et database-repository og en DbContext pr. request. Data gemmes i SQL.

Controlleren og frontenden behøver ingen ændringer, når der skiftes.
Der overføres ikke automatisk data fra listen til databasen. Den første overførsel
af de fire eksisterende numre er udført én gang ved opsætningen i Azure.

## Opret tabellen

Migrationen `InitialMusicDatabase` opretter `Collections` med et automatisk Id.
SQL-scriptet `scripts/CreateMusicDatabase.sql` er genereret fra migrationen og
kontrollerer migrationshistorikken, så samme migration ikke anvendes igen.
Kør det på den valgte musikdatabase, før API'et skiftes til Database.

Alternativt, fra projektets rod:

```powershell
dotnet tool restore
# Indstil ConnectionStrings__MusicDatabase sikkert i dit lokale miljø først.
dotnet ef database update --project RESTmusic/RESTmusic.csproj
```

Migrationsværktøjet læser `ConnectionStrings__MusicDatabase`. Hvis den mangler,
vælger værktøjet en lokal LocalDB-database ved navn MusicCollection.
API'et bruger aldrig denne lokale standard; API'et kræver en indstillet forbindelse.

## Azure

1. Opret/vælg en Azure SQL Database, og kontrollér pris og abonnement.
2. Giv backend adgang til databasen med den valgte godkendelses- og netværksopsætning.
3. Kør SQL-scriptet i musikdatabasen.
4. Indstil forbindelsen i backendens App Service under connection strings:
   navn `MusicDatabase`, type `SQLAzure`.
5. Publicér den opdaterede backend og kontrollér, at login stadig virker med List.
6. Sæt backendens app-indstilling `Repository__Type` til `Database` og gem.
7. Tilføj et nummer gennem frontenden. Genstart backend og kontrollér, at nummeret findes.

Databasen starter tom. Tilføj numre via frontenden efter login.
Gem ikke adgangskoder eller den rigtige connection string i GitHub eller frontend.
JWT-indstillingerne skal blive stående i backendens App Service.

### Opsætningen i Azure-portalen

Databasen bruger serveren `restmusic-sofie-20261104` i Sweden Central og
databasen `MusicCollection`. Gratis-tilbuddet skal have `Overage billing: Disabled`.
SQL-serveren bruger Microsoft Entra-login med skolekontoen som administrator.
Backendens systemtildelte managed identity er allerede aktiveret.
Den godkendte browser-IP er tilladt i serverens firewall. Azure-tjenester er også
tilladt på netværket; Microsoft Entra-godkendelse kræves stadig til databasen.

Når databasen og tabellen er oprettet, kan administratoren køre
`scripts/GrantBackendAccess.sql`. Det giver kun backend læse- og skriverettigheder
til musiktabellen. Selve tildelingen skal være godkendt, før scriptet køres.

Backendens gemte forbindelse bruger managed identity uden adgangskode:

```text
Server=tcp:restmusic-sofie-20261104.database.windows.net,1433;Database=MusicCollection;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Forbindelsen ovenfor er til Azure-backenden. Lokal udvikling kræver i stedet en
lokal database eller et Microsoft Entra-login med databaseadgang.

## Nye tabelændringer

```powershell
dotnet ef migrations add BeskrivAendringen --project RESTmusic/RESTmusic.csproj
dotnet ef migrations script --idempotent --project RESTmusic/RESTmusic.csproj --output scripts/CreateMusicDatabase.sql
```

Se SQL-scriptet igennem før det køres på en database med rigtige data.
