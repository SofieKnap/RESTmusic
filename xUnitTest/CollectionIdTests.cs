using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    // De samme id-regler kontrolleres på både listen og en rigtig SQLite-testdatabase.
    public class CollectionIdTests : IDisposable
    {
        private SqliteConnection? connection;
        private CollectionDbContext? context;

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Add_TildelerPositiveOgUnikkeIdAutomatisk(bool database)
        {
            var repo = OpretRepository(database);

            var first = repo.AddCollection(NytNummer("Et"));
            var second = repo.AddCollection(NytNummer("To"));

            Assert.True(first.Id > 0);
            Assert.True(second.Id > 0);
            Assert.NotEqual(first.Id, second.Id);
            var ids = repo.GetAllCollections().Select(album => album.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Add_IgnorererIdFraKlienten(bool database)
        {
            var repo = OpretRepository(database);
            var input = NytNummer("Test");
            input.Id = int.MaxValue;

            var saved = repo.AddCollection(input);

            Assert.True(saved.Id > 0);
            Assert.NotEqual(input.Id, saved.Id);
            Assert.Same(saved, repo.GetCollection(saved.Id));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Update_BevarerIdSelvOmTitelKunstnerOgBodyIdAendres(bool database)
        {
            var repo = OpretRepository(database);
            var saved = repo.AddCollection(NytNummer("Oprindelig"));
            int id = saved.Id;
            var updated = NytNummer("Ny titel");
            updated.Artist = "Ny kunstner";
            updated.Id = 999999;

            var result = repo.UpdateCollection(id, updated);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Ny titel", repo.GetCollection(id)!.Title);
            Assert.Equal("Ny kunstner", repo.GetCollection(id)!.Artist);
            Assert.Null(repo.GetCollection(999999));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Delete_GenbrugerIkkeDetSlettedeId(bool database)
        {
            var repo = OpretRepository(database);
            var first = repo.AddCollection(NytNummer("Et"));
            var deleted = repo.AddCollection(NytNummer("To"));
            int deletedId = deleted.Id;

            repo.DeleteCollection(deletedId);
            var next = repo.AddCollection(NytNummer("Tre"));

            Assert.Null(repo.GetCollection(deletedId));
            Assert.True(next.Id > deletedId);
            Assert.NotNull(repo.GetCollection(first.Id));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Add_SammeInputObjektKanIkkeAendreEtEksisterendeId(bool database)
        {
            var repo = OpretRepository(database);
            var input = NytNummer("Test");

            var first = repo.AddCollection(input);
            int firstId = first.Id;
            var second = repo.AddCollection(input);

            Assert.Equal(firstId, first.Id);
            Assert.NotEqual(firstId, second.Id);
            Assert.NotNull(repo.GetCollection(firstId));
        }

        [Fact]
        public void Liste_SamtidigeTilfoejelserFaarUnikkeId()
        {
            var repo = new CollectionRepositoryList();
            int before = repo.GetAllCollections().Count();

            Parallel.For(0, 50, index => repo.AddCollection(NytNummer("Test " + index)));

            var ids = repo.GetAllCollections().Select(album => album.Id).ToList();
            Assert.Equal(before + 50, ids.Count);
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        [Fact]
        public void Database_IdOgOplysningerErGemtUdenForChangeTracker()
        {
            var repo = OpretRepository(true);
            var saved = repo.AddCollection(NytNummer("Gemt"));
            int id = saved.Id;
            context!.ChangeTracker.Clear();

            var loaded = repo.GetCollection(id);

            Assert.NotNull(loaded);
            Assert.Equal(id, loaded.Id);
            Assert.Equal("Gemt", loaded.Title);
            Assert.NotSame(saved, loaded);
        }

        [Theory]
        [InlineData("LOW", null, 1)]
        [InlineData(null, "niki", 1)]
        [InlineData("low", "SZA", 0)]
        public void Database_FilterKanUdfoeresSomSql(string? title, string? artist, int expected)
        {
            var repo = OpretRepository(true);
            var niki = NytNummer("lowkey");
            niki.Artist = "NIKI";
            repo.AddCollection(niki);
            var sza = NytNummer("The Weekend");
            sza.Artist = "SZA";
            repo.AddCollection(sza);

            var result = repo.GetAllCollections(title, artist).ToList();

            Assert.Equal(expected, result.Count);
        }

        private ICollectionRepository OpretRepository(bool database)
        {
            if (!database) return new CollectionRepositoryList();
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<CollectionDbContext>().UseSqlite(connection).Options;
            context = new CollectionDbContext(options);
            context.Database.EnsureCreated();
            return new CollectionRepositoryDB(context);
        }

        private static Collection NytNummer(string title)
        {
            return new Collection { Title = title, Artist = "Testkunstner", Duration = 180, PublicationYear = 2026 };
        }

        public void Dispose()
        {
            context?.Dispose();
            connection?.Dispose();
        }
    }
}
