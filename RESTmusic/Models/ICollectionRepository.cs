namespace RESTmusic.Models
{
    public interface ICollectionRepository
    {
        Collection AddCollection(Collection collection);
        IEnumerable<Collection> GetAllCollections();

    }
}
