using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    public class CollectionRepositoryTests
    {
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
            Assert.Same(album, repo.GetCollection("Testnummer", "Testkunstner"));
        }

        [Fact]
        public void GetCollection_SkelnerMellemSammeTitelMedForskelligeKunstnere()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var andetNummer = new Collection { Title = "lowkey", Artist = "Anden kunstner", Duration = 200, PublicationYear = 2026 };
            repo.AddCollection(andetNummer);

            // Act
            var result = repo.GetCollection("lowkey", "Anden kunstner");

            // Assert
            Assert.Same(andetNummer, result);
            Assert.Equal("NIKI", repo.GetCollection("lowkey", "NIKI")!.Artist);
        }

        [Theory]
        [InlineData("low", "NIKI")]
        [InlineData("lowkey", "Forkert kunstner")]
        public void GetCollection_ReturnererNullUdenPraecistMatch(string title, string artist)
        {
            // Arrange
            var repo = new CollectionRepositoryList();

            // Act
            var result = repo.GetCollection(title, artist);

            // Assert: Opslag af ét nummer kræver hele titlen og den rigtige kunstner.
            Assert.Null(result);
        }

        [Fact]
        public void DeleteCollection_FjernerKunDetValgteNummer()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var andetNummer = new Collection { Title = "lowkey", Artist = "Anden kunstner", Duration = 200, PublicationYear = 2026 };
            repo.AddCollection(andetNummer);
            int antalFoer = repo.GetAllCollections().Count();

            // Act
            var result = repo.DeleteCollection("lowkey", "NIKI");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NIKI", result.Artist);
            Assert.Null(repo.GetCollection("lowkey", "NIKI"));
            Assert.Same(andetNummer, repo.GetCollection("lowkey", "Anden kunstner"));
            Assert.Equal(antalFoer - 1, repo.GetAllCollections().Count());
        }

        [Fact]
        public void DeleteCollection_ManglendeNummerAendrerIkkeSamlingen()
        {
            // Arrange
            var repo = new CollectionRepositoryList();
            var numreFoer = repo.GetAllCollections().ToList();

            // Act
            var result = repo.DeleteCollection("Findes ikke", "Ingen");

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
            var result = repo.UpdateCollection("lowkey", "NIKI", updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ny titel", result.Title);
            Assert.Equal("Ny kunstner", result.Artist);
            Assert.Equal(240, result.Duration);
            Assert.Equal(2026, result.PublicationYear);
            Assert.Null(repo.GetCollection("lowkey", "NIKI"));
            Assert.Same(result, repo.GetCollection("Ny titel", "Ny kunstner"));
            Assert.Equal(192, repo.GetCollection("Gabriela", "KATSEYE")!.Duration);
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
            var result = repo.UpdateCollection("Findes ikke", "Ingen", updated);

            // Assert
            Assert.Null(result);
            Assert.Null(repo.GetCollection("Ny titel", "Ny kunstner"));
            Assert.Equal(antalFoer, repo.GetAllCollections().Count());
            Assert.Equal(185, repo.GetCollection("lowkey", "NIKI")!.Duration);
        }
    }
}
