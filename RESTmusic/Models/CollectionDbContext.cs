using Microsoft.EntityFrameworkCore;

namespace RESTmusic.Models
{
    public class CollectionDbContext : DbContext
    {
        // Program.cs giver context oplysningerne om databaseforbindelsen.
        public CollectionDbContext(DbContextOptions<CollectionDbContext> options)
            : base(options)
        {
        }

        // Collections repræsenterer tabellen med musiknumre.
        public DbSet<Collection> Collections => Set<Collection>();
    }
}
