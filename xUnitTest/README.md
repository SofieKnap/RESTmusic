# Backendtests

Projektet har 46 xUnit-testkørsler fordelt på controller, repository, modelvalidering, login og de øvrige modeller.

## Kør i Visual Studio

Stop først den lokale backend, hvis den kører. Vælg derefter **Test → Test Explorer → Run All** i RESTmusic-løsningen.

Hvis RESTmusic.exe er låst, kan projektet ikke bygges. Brug **Shift + F5** til at stoppe debugkørslen. En backend startet uden debugger skal lukkes i sit konsolvindue.

Fra terminalen kan testene køres med:

```powershell
dotnet test xUnitTest/RepositoryTest.csproj
```

Der kræves ingen browser, database eller Azure-forbindelse. Hver test opretter sine egne data, så testene ikke ændrer den kørende app.

## Hvad kontrolleres?

- `CollectionControllerTests`: CRUD, 201/200/404/409, dubletter og adresser til oprettede numre.
- `CollectionRepositoryTests`: opslag, titel- og kunstnerfiltre, oprettelse, opdatering og sletning samt numre, der ikke findes.
- `CollectionValidationTests`: null, tomme felter, ugyldige tal og tilladte grænseværdier.
- `AuthControllerTests`: forkerte loginoplysninger, et gyldigt signeret Admin-token, tokenets levetid og forskellige token-id'er.
- `ModelTests`: musikkens tekstvisning og standardeksemplet WeatherForecast.
- `UnitTest1`: kontrol af, at startdata indeholder musik.

Login-testene giver controlleren en konfiguration med en nøgle, der kun bruges til tests. De opsætter ikke login i den rigtige app. `Program.cs` mangler fortsat JWT-authentication og appens JWT-indstillinger, så Admin-beskyttet DELETE er ikke verificeret gennem HTTP-pipelinen. Controllerens DELETE-logik er testet direkte.

`CollectionRepositoryDB` er en tom klasse og har endnu ingen databasefunktioner at teste.

Frontendens Selenium-tests ligger i den separate MusicTest-løsning. De kræver Live Server og adgang til Azure-API'et.
