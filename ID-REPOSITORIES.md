# Automatiske id'er

Hvert musiknummer har et positivt heltals-id. Man skal ikke angive id ved oprettelse, og titel eller kunstner kan ændres uden at ændre id'et.

- Liste-repositoryet bruger en tæller. Den øges ved hver oprettelse og går ikke baglæns efter sletning. Samtidige tilføjelser er beskyttet af en lås.
- Database-repositoryet lader Entity Framework og databasen generere id'et ved indsættelse. Det er primærnøglen, der sikrer, at samme id ikke kan bruges til flere rækker.
- Et id sendt i POST-body ignoreres. Ved PUT er det id'et i adressen, der gælder; body kan ikke ændre det.
- Frontend gemmer id'et sammen med de hentede musikdata og sender det automatisk ved klik på Opdater eller Slet. Id'et vises ikke som et felt, brugeren skal kende.

## Adresser

```text
GET    /api/Collection                 Hele samlingen og valgfri filtre
GET    /api/Collection/17              Ét bestemt nummer
POST   /api/Collection                 Opret et nyt nummer uden selv at vælge id
PUT    /api/Collection/17              Ret nummer 17, behold id'et
DELETE /api/Collection/17              Slet nummer 17
```

POST, PUT og DELETE kræver fortsat et Admin-token. Søgning sker stadig med `?title=...&artist=...`.

## Liste og database

Id'er er unikke i den aktuelle samling. Listen ligger i hukommelsen, så både indhold og tæller bliver nulstillet, når appen genstarter. En database gemmer data og id'er mellem genstarter.

SQL Server/Azure SQL kan vælges med `Repository:Type = Database` og forbindelsen `MusicDatabase`. Standardvalget er fortsat `List`. Migrationen og SQL-scriptet opretter tabellen, men der er ikke automatisk oprettet eller tilsluttet en database i Azure. Se [databaseopsætningen](DATABASE-SETUP.md). Database-repositoryets id-adfærd er også testet med SQLite i hukommelsen.

## Publicering

Publicér backend først. Upload derefter den opdaterede `index.js` fra FrontMusic, og genindlæs siden. Den tidligere API-udgave med titel/kunstner i adressen passer ikke til den nye frontend.
