namespace RESTmusic.Models
{
    public class CollectionRepositoryList : ICollectionRepository
    {
        private readonly List<Collection> _collections = new List<Collection>();
        
        public CollectionRepositoryList()
        {
            AddCollection(new Collection { Title = "lowkey", Artist = "NIKI", Duration = 185, PublicationYear = 2019 });
            AddCollection(new Collection { Title = "The Weekend", Artist = "SZA", Duration = 240, PublicationYear = 2022 });
            AddCollection(new Collection { Title = "Gabriela", Artist = "KATSEYE", Duration = 192, PublicationYear = 2025 });
            AddCollection(new Collection { Title = "¨That's So True", Artist = "Gracie Abrams", Duration = 186, PublicationYear = 2024 });
        }

        public Collection AddCollection(Collection collection)
        {
            _collections.Add(collection);
            return collection;
        }

        public IEnumerable<Collection> GetAllCollections(string? title = null, string? artist = null)
        {
            IEnumerable<Collection> result = _collections;
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

        public Collection? GetCollection(string title, string artist)
        {
            return _collections.FirstOrDefault(c => c.Title == title && c.Artist == artist);
        }

        public Collection? DeleteCollection(string title, string artist)
        {
            var collection = GetCollection(title, artist);
            if (collection != null)
            {
                _collections.Remove(collection);
            }
            return collection;
        }

        public Collection? UpdateCollection(string title, string artist, Collection updatedCollection)
        {
            var collection = GetCollection(title, artist);
            if (collection != null)
            {
                collection.Title = updatedCollection.Title;
                collection.Artist = updatedCollection.Artist;
                collection.Duration = updatedCollection.Duration;
                collection.PublicationYear = updatedCollection.PublicationYear;
            }
            return collection;
        }
    }
}
