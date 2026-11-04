using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RESTmusic.Models
{
    // Bruges kun af migrationsværktøjet, så det ikke skal starte API'et eller bruge JWT.
    public class CollectionDbContextFactory : IDesignTimeDbContextFactory<CollectionDbContext>
    {
        public CollectionDbContext CreateDbContext(string[] args)
        {
            string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MusicDatabase")
                ?? @"Server=(localdb)\MSSQLLocalDB;Database=MusicCollection;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=False";
            var options = new DbContextOptionsBuilder<CollectionDbContext>()
                .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
                .Options;
            return new CollectionDbContext(options);
        }
    }
}
