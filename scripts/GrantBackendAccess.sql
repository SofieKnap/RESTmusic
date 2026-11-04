-- Kør som databasens Microsoft Entra-administrator i MusicCollection.
-- Opret først Collections med CreateMusicDatabase.sql.
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'RESTmusicProject')
BEGIN
    CREATE USER [RESTmusicProject] FROM EXTERNAL PROVIDER;
END;
GO

-- Backend må kun læse og ændre musikdata; den behøver ikke administratorrettigheder.
GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.Collections TO [RESTmusicProject];
GO
