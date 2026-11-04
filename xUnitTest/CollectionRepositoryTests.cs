using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    public class CollectionRepositoryTests
    {
        // theory bruges når samme test køres med flere sæt testdata.
        [Theory]
        [InlineData("LOW", null)]
        [InlineData(null, "niki")]
        [InlineData("low", "NIKI")]
        public void GetAllCollections_FinderNummerMedDelvisTitelOgKunstner(string? title, string? artist)
        {
            // Arrange
            var repo = new CollectionRepositoryList();

            // Act
            var result = repo.GetAllCollections(title, artist);

            // Assert: Søgningen ignorerer store og små bogstaver.
            var album = Assert.Single(result);
            Assert.Equal("lowkey", album.Title);
            Assert.Equal("NIKI", album.Artist);
        }

        [Theory]
        [InlineData("low", "SZA")]
        [InlineData("Findes ikke", null)]
        public void GetAllCollections_ReturnererTomListeUdenMatch(string title, string? artist)
        {
            // Arrange
            var repo = new CollectionRepositoryList();

            // Act: Når begge filtre bruges, skal begge passe på samme nummer.
            var result = repo.GetAllCollections(title, artist);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public void GetAllCollections_TommeFiltreReturnererHeleSamlingen(string title, string artist)
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var alleNumre = repo.GetAllCollections().ToList();

            // Act
            var result = repo.GetAllCollections(title, artist).ToList();

            // Assert
            Assert.Equal(alleNumre, result);
        }

        [Fact]
        public void AddCollection_TilfoejerEtNummerSomKanHentesIgen()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            int antalFoer = repo.GetAllCollections().Count();
            var album = new Collection { Title = "Testnummer", Artist = "Testkunstner", Duration = 180, PublicationYear = 2026 };

            // Act
            var result = repo.AddCollection(album);

            // Assert
            Assert.Equal(antalFoer + 1, repo.GetAllCollections().Count());
            Assert.Equal(album.Title, result.Title);
            Assert.True(result.Id > 0);
            Assert.Same(result, repo.GetCollection(result.Id));
        }

        [Fact]
        public void GetCollection_SkelnerMellemSammeTitelMedForskelligeKunstnere()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var andetNummer = new Collection { Title = "lowkey", Artist = "Anden kunstner", Duration = 200, PublicationYear = 2026 };
            var created = repo.AddCollection(andetNummer);

            // Act
            var result = repo.GetCollection(created.Id);

            // Assert
            Assert.Same(created, result);
            Assert.Equal("NIKI", repo.GetAllCollections("lowkey", "NIKI").Single().Artist);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public void GetCollection_ReturnererNullForUkendtId(int id)
        {
            // Arrange
            var repo = new CollectionRepositoryList();

            // Act
            var result = repo.GetCollection(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteCollection_FjernerKunDetValgteNummer()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var andetNummer = new Collection { Title = "lowkey", Artist = "Anden kunstner", Duration = 200, PublicationYear = 2026 };
            var created = repo.AddCollection(andetNummer);
            int antalFoer = repo.GetAllCollections().Count();

            // Act
            int id = repo.GetAllCollections("lowkey", "NIKI").Single().Id;
            var result = repo.DeleteCollection(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NIKI", result.Artist);
            Assert.Null(repo.GetCollection(id));
            Assert.Same(created, repo.GetCollection(created.Id));
            Assert.Equal(antalFoer - 1, repo.GetAllCollections().Count());
        }

        [Fact]
        public void DeleteCollection_ManglendeNummerAendrerIkkeSamlingen()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var numreFoer = repo.GetAllCollections().ToList();

            // Act
            var result = repo.DeleteCollection(int.MaxValue);

            // Assert
            Assert.Null(result);
            Assert.Equal(numreFoer, repo.GetAllCollections().ToList());
        }

        [Fact]
        public void UpdateCollection_OpdatererAlleFelterOgBevarerAndreNumre()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            int antalFoer = repo.GetAllCollections().Count();
            var updated = new Collection { Title = "Ny titel", Artist = "Ny kunstner", Duration = 240, PublicationYear = 2026 };

            // Act
            int id = repo.GetAllCollections("lowkey", "NIKI").Single().Id;
            var result = repo.UpdateCollection(id, updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ny titel", result.Title);
            Assert.Equal("Ny kunstner", result.Artist);
            Assert.Equal(240, result.Duration);
            Assert.Equal(2026, result.PublicationYear);
            Assert.Equal(id, result.Id);
            Assert.Same(result, repo.GetCollection(id));
            Assert.Equal(192, repo.GetAllCollections("Gabriela", "KATSEYE").Single().Duration);
            Assert.Equal(antalFoer, repo.GetAllCollections().Count());
        }

        [Fact]
        public void UpdateCollection_ManglendeNummerOpretterIkkeEtNyt()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            int antalFoer = repo.GetAllCollections().Count();
            var updated = new Collection { Title = "Ny titel", Artist = "Ny kunstner", Duration = 240, PublicationYear = 2026 };

            // Act
            var result = repo.UpdateCollection(int.MaxValue, updated);

            // Assert
            Assert.Null(result);
            Assert.Empty(repo.GetAllCollections("Ny titel", "Ny kunstner"));
            Assert.Equal(antalFoer, repo.GetAllCollections().Count());
            Assert.Equal(185, repo.GetAllCollections("lowkey", "NIKI").Single().Duration);
        }
    }
}
