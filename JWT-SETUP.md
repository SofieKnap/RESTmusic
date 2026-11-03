# JWT-login i RESTmusic

Login bruger den eksisterende skolekonto: brugernavn `admin` og adgangskode `1234`.

- GET er åben uden login.
- POST, PUT og DELETE kræver et gyldigt token med rollen Admin.
- Frontenden sender tokenet som `Authorization: Bearer ...`.
- Tokenet udløber efter to timer. Login igen for at få et nyt.
- Log ud i frontend fjerner tokenet fra denne side. Et allerede udstedt token er gyldigt indtil udløb.

## Lokalt

Start med Visual Studios `http`- eller `https`-profil. De vælger Development-miljøet, hvor appen automatisk laver en tilfældig signeringsnøgle i hukommelsen. En genstart giver en ny nøgle, og tidligere tokens skal derfor erstattes ved et nyt login.

`Jwt:Issuer` er `RESTmusic`, og `Jwt:Audience` er `FrontMusic`. De står i appsettings.json og bruges både ved udstedelse og validering.

Frontend bruger fortsat Azure-API'et. Lokalt fungerende JWT bliver først tilgængeligt på hjemmesiden, når backend publiceres igen.

## Azure

1. Kør følgende fra repositoryets rodmappe i PowerShell:

   ```powershell
   .\scripts\New-JwtKey.ps1
   ```

   Det kopierer en ny tilfældig nøgle til udklipsholderen.

2. Åbn backendens Azure Web App, **RESTmusicProject**. Gå til **Settings → Environment variables → App settings** (i nogle visninger hedder det Configuration).
3. Tilføj en indstilling med navnet **Jwt__Key**. Indsæt den kopierede nøgle som værdi, og gem/apply ændringen.
4. Publicér den opdaterede RESTmusic-backend igen med jeres eksisterende publiceringsmetode. Vent på, at appen er genstartet, og prøv **Log ind** på frontend.

Der skal ikke stå en hemmelig signeringsnøgle i GitHub eller frontend. Hvis Jwt__Key mangler i Production, stopper appen ved opstart med en forklaring. Den bruger ikke automatisk en udviklingsnøgle på Azure.

Issuer og Audience læses fra den publicerede appsettings.json. Hvis I selv sætter Jwt__Issuer og Jwt__Audience i Azure, skal værdierne være henholdsvis RESTmusic og FrontMusic, medmindre I bevidst vil ændre dem.

## Test

Kør `dotnet test xUnitTest/RepositoryTest.csproj`.

Der er 60 testkørsler, inklusive HTTP-integrationstests af login, læsning uden login, 401/403, ugyldige tokens, oprettelse/opdatering/sletning med Admin-token samt CORS med Authorization-header. Integrationstestene bruger en isoleret server i hukommelsen og ændrer ingen data på Azure.
