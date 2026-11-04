# Backendtests

Projektet har 76 testkørsler (unittests og HTTP-integrationstests) fordelt på controller, repository, modelvalidering, login og de øvrige modeller.

## Kør i Visual Studio

Stop først den lokale backend, hvis den kører. Vælg derefter **Test → Test Explorer → Run All** i RESTmusic-løsningen.

Hvis RESTmusic.exe er låst, kan projektet ikke bygges. Brug **Shift + F5** til at stoppe debugkørslen. En backend startet uden debugger skal lukkes i sit konsolvindue.

Fra terminalen kan testene køres med:

```powershell
dotnet test xUnitTest/RepositoryTest.csproj
```

Der kræves ingen browser, ekstern database eller Azure-forbindelse. Hver test opretter sine egne data, så testene ikke ændrer den kørende app.

## Hvad kontrolleres?

- `CollectionControllerTests`: CRUD, 201/200/404/409, dubletter og adresser til oprettede numre.
- `CollectionIdTests`: samme id-regler i liste og SQLite, stabile id’er ved opdatering, ingen genbrug efter sletning og samtidige tilføjelser til listen.
- `CollectionRepositoryTests`: opslag, titel- og kunstnerfiltre, oprettelse, opdatering og sletning samt numre, der ikke findes.
- `CollectionValidationTests`: null, tomme felter, ugyldige tal og tilladte grænseværdier.
- `AuthControllerTests`: forkerte loginoplysninger, et gyldigt signeret Admin-token, tokenets levetid og forskellige token-id'er.
- `ModelTests`: musikkens tekstvisning og standardeksemplet WeatherForecast.
- `UnitTest1`: kontrol af, at startdata indeholder musik.

Login-unittestene bruger en testnøgle. `JwtIntegrationTests` starter den rigtige app i en isoleret testserver og kontrollerer også JWT-validering, Admin-rollen, CORS og POST/PUT/DELETE gennem HTTP-pipelinen. Se JWT-SETUP.md i repositoryets rodmappe for den nødvendige Azure-indstilling Jwt__Key.

`CollectionRepositoryDB` testes mod en midlertidig SQLite-database i hukommelsen. Det opretter ingen rigtig databaseforbindelse i Program.cs. Appen bruger stadig liste-repositoryet, indtil databaseudbyder og forbindelse er valgt.

Frontendens Selenium-tests ligger i den separate MusicTest-løsning. De kræver Live Server og adgang til Azure-API'et.
