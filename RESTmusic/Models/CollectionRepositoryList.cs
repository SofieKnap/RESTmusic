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
            var collections = _collections.AsEnumerable();
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
    }
}
