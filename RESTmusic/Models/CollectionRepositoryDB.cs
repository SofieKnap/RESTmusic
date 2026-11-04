namespace RESTmusic.Models
{
    public class CollectionRepositoryDB : ICollectionRepository
    {
        private readonly CollectionDbContext _context;

        public CollectionRepositoryDB(CollectionDbContext context)
        {
            _context = context;
        }

        public Collection AddCollection(Collection collection)
        {
            // Kopiér kun musikoplysningerne. Databasen bestemmer selv Id.
            var created = new Collection
            {
                Title = collection.Title,
                Artist = collection.Artist,
                Duration = collection.Duration,
                PublicationYear = collection.PublicationYear
            };
            _context.Collections.Add(created);
            _context.SaveChanges();
            return created;
        }

        public Collection? DeleteCollection(int id)
        {
            var collection = _context.Collections.FirstOrDefault(c => c.Id == id);
            if (collection != null)
            {
                _context.Collections.Remove(collection);
                _context.SaveChanges();
            }
            return collection;
        }

        public IEnumerable<Collection> GetAllCollections(string? title = null, string? artist = null)
        {
            var query = _context.Collections.AsQueryable();
            if (!string.IsNullOrWhiteSpace(title))
            {
                string searchTitle = title.ToLowerInvariant();
                query = query.Where(c => c.Title.ToLower().Contains(searchTitle));
            }
            if (!string.IsNullOrWhiteSpace(artist))
            {
                string searchArtist = artist.ToLowerInvariant();
                query = query.Where(c => c.Artist.ToLower().Contains(searchArtist));
            }
            return query.ToList();
        }

        public Collection? GetCollection(int id)
        {
            return _context.Collections.FirstOrDefault(c => c.Id == id);
        }

        public Collection? UpdateCollection(int id, Collection updatedCollection)
        {
            var collection = _context.Collections.FirstOrDefault(c => c.Id == id);
            if (collection != null)
            {
                collection.Title = updatedCollection.Title;
                collection.Artist = updatedCollection.Artist;
                collection.Duration = updatedCollection.Duration;
                collection.PublicationYear = updatedCollection.PublicationYear;
                _context.SaveChanges();
            }
            return collection;
        }
    }
}
