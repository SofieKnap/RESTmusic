namespace RESTmusic.Models
{
    public class CollectionRepositoryList
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

        public IEnumerable<Collection> GetAllCollections()
        {
            return _collections.ToList();
        }
    }
}
