namespace RESTmusic.Models
{
    public interface ICollectionRepository
    {
        Collection AddCollection(Collection collection);
        IEnumerable<Collection> GetAllCollections(string? title=null, string? artist=null);
        Collection? GetCollection(string title, string artist);

        Collection? DeleteCollection(string title, string artist);
        Collection? UpdateCollection(string title, string artist, Collection updatedCollection);
    }

}
