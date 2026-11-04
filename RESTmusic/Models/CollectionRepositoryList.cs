namespace RESTmusic.Models
{
    public class CollectionRepositoryList : ICollectionRepository
    {
        private readonly List<Collection> _collections = new List<Collection>();
        private readonly object _lock = new object();
        private int _nextId = 1;
        
        public CollectionRepositoryList()
        {
            AddCollection(new Collection { Title = "lowkey", Artist = "NIKI", Duration = 185, PublicationYear = 2019 });
            AddCollection(new Collection { Title = "The Weekend", Artist = "SZA", Duration = 240, PublicationYear = 2022 });
            AddCollection(new Collection { Title = "Gabriela", Artist = "KATSEYE", Duration = 192, PublicationYear = 2025 });
            AddCollection(new Collection { Title = "¨That's So True", Artist = "Gracie Abrams", Duration = 186, PublicationYear = 2024 });
        }

        public Collection AddCollection(Collection collection)
        {
            // Låsen sikrer unikke id'er, også ved samtidige HTTP-kald.
            lock (_lock)
            {
                var created = new Collection
                {
                    Id = _nextId++,
                    Title = collection.Title,
                    Artist = collection.Artist,
                    Duration = collection.Duration,
                    PublicationYear = collection.PublicationYear
                };
                _collections.Add(created);
                return created;
            }
        }

        public IEnumerable<Collection> GetAllCollections(string? title = null, string? artist = null)
        {
            // Tag et øjebliksbillede, så listen kan læses under andre kald.
            IEnumerable<Collection> result;
            lock (_lock)
            {
                result = _collections.ToList();
            }
            //filtrer by title 
            if (!string.IsNullOrWhiteSpace(title))
            {
                result = result.Where(c => c.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }
            //filtrer by artist
            if (!string.IsNullOrWhiteSpace(artist))
            {
                result = result.Where(c => c.Artist.Contains(artist, StringComparison.OrdinalIgnoreCase));
            }
            return result.ToList();
        }

        public Collection? GetCollection(int id)
        {
            lock (_lock)
            {
                return _collections.FirstOrDefault(c => c.Id == id);
            }
        }

        public Collection? DeleteCollection(int id)
        {
            lock (_lock)
            {
                var collection = GetCollection(id);
                if (collection != null)
                {
                    _collections.Remove(collection);
                }
                return collection;
            }
        }

        public Collection? UpdateCollection(int id, Collection updatedCollection)
        {
            lock (_lock)
            {
                var collection = GetCollection(id);
                if (collection != null)
                {
                    // Id'et beholdes, også hvis titel og kunstner ændres.
                    collection.Title = updatedCollection.Title;
                    collection.Artist = updatedCollection.Artist;
                    collection.Duration = updatedCollection.Duration;
                    collection.PublicationYear = updatedCollection.PublicationYear;
                }
                return collection;
            }
        }
    }
}
