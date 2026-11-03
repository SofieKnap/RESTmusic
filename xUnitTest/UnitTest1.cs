using Xunit;
using RESTmusic.Models;

namespace xUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void TestGetAllCollections()
        {
            // Arrange
            var repo = new CollectionRepositoryList();

            // Act
            var collections = repo.GetAllCollections();

            // Assert
            Assert.NotEmpty(collections);
            Assert.Contains(collections, album => album.Title == "lowkey" && album.Artist == "NIKI");
        }
    }
}
