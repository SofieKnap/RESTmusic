namespace RESTmusic.Models
{
    public interface ICollectionRepository
    {
        Collection AddCollection(Collection collection);
        IEnumerable<Collection> GetAllCollections(string? title=null, string? artist=null);
        Collection? GetCollection(int id);

        Collection? DeleteCollection(int id);
        Collection? UpdateCollection(int id, Collection updatedCollection);
    }

}
