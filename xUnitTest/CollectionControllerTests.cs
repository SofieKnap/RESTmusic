using Microsoft.AspNetCore.Mvc;
using RESTmusic.Controllers;
using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    public class CollectionControllerTests
    {
        [Fact]
        public void GetAllCollections_FiltrererPaaTitelOgKunstner()
        {
            var controller = new CollectionController(new CollectionRepositoryList());

            var response = controller.GetAllCollections("LOW", "niki");

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            var albums = Assert.IsAssignableFrom<IEnumerable<Collection>>(ok.Value);
            var album = Assert.Single(albums);
            Assert.Equal("lowkey", album.Title);
            Assert.Equal("NIKI", album.Artist);
        }

        [Fact]
        public void GetCollection_ReturnererDetPraeciseNummer()
        {
            var controller = new CollectionController(new CollectionRepositoryList());

            var response = controller.GetCollection("lowkey", "NIKI");

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            var album = Assert.IsType<Collection>(ok.Value);
            Assert.Equal("lowkey", album.Title);
            Assert.Equal("NIKI", album.Artist);
        }

        [Fact]
        public void GetCollection_Returnerer404NaarKunEnDelAfTitlenMatcher()
        {
            var controller = new CollectionController(new CollectionRepositoryList());

            var response = controller.GetCollection("low", "NIKI");

            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public void Post_OpretterNummerOgReturnerer201MedGetAdresse()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            var album = new Collection { Title = "Testnummer", Artist = "Testkunstner", Duration = 180, PublicationYear = 2026 };

            var response = controller.Post(album);

            var created = Assert.IsType<CreatedAtActionResult>(response.Result);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal(nameof(CollectionController.GetCollection), created.ActionName);
            Assert.Equal(album.Title, created.RouteValues!["title"]);
            Assert.Equal(album.Artist, created.RouteValues["artist"]);
            Assert.Same(album, created.Value);
            Assert.Same(album, repo.GetCollection(album.Title, album.Artist));
        }

        [Fact]
        public void Post_Returnerer409VedDublet()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            int countBefore = repo.GetAllCollections().Count();
            var album = new Collection { Title = "lowkey", Artist = "NIKI", Duration = 180, PublicationYear = 2026 };

            var response = controller.Post(album);

            Assert.IsType<ConflictObjectResult>(response.Result);
            Assert.Equal(countBefore, repo.GetAllCollections().Count());
        }

        [Fact]
        public void Put_OpdatererAlleOplysningerOgSkifterNoeglen()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            var updated = new Collection { Title = "Ny titel", Artist = "Ny kunstner", Duration = 200, PublicationYear = 2026 };

            var response = controller.Put("lowkey", "NIKI", updated);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            var album = Assert.IsType<Collection>(ok.Value);
            Assert.Equal(updated.Title, album.Title);
            Assert.Equal(updated.Artist, album.Artist);
            Assert.Equal(updated.Duration, album.Duration);
            Assert.Equal(updated.PublicationYear, album.PublicationYear);
            Assert.Null(repo.GetCollection("lowkey", "NIKI"));
            Assert.Same(album, repo.GetCollection(updated.Title, updated.Artist));
        }

        [Fact]
        public void Put_KanBeholdeTitelOgKunstner()
        {
            var controller = new CollectionController(new CollectionRepositoryList());
            var updated = new Collection { Title = "lowkey", Artist = "NIKI", Duration = 200, PublicationYear = 2019 };

            var response = controller.Put("lowkey", "NIKI", updated);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(200, Assert.IsType<Collection>(ok.Value).Duration);
        }

        [Fact]
        public void Put_Returnerer404NaarNummeretIkkeFindes()
        {
            var controller = new CollectionController(new CollectionRepositoryList());
            var updated = new Collection { Title = "Ny titel", Artist = "Ny kunstner", Duration = 200, PublicationYear = 2026 };

            var response = controller.Put("Findes ikke", "Ingen", updated);

            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public void Put_Returnerer409UdenAtAendreDetOprindeligeNummer()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            var updated = new Collection { Title = "Gabriela", Artist = "KATSEYE", Duration = 1, PublicationYear = 2026 };

            var response = controller.Put("lowkey", "NIKI", updated);

            Assert.IsType<ConflictObjectResult>(response.Result);
            Assert.Equal(185, repo.GetCollection("lowkey", "NIKI")!.Duration);
            Assert.Equal(192, repo.GetCollection("Gabriela", "KATSEYE")!.Duration);
        }

        // Disse tests kalder metoden direkte. Login og Admin-rollen håndhæves af HTTP-pipelinen.
        [Fact]
        public void Delete_FjernerNummeretOgReturnererDet()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            int countBefore = repo.GetAllCollections().Count();

            var response = controller.Delete("lowkey", "NIKI");

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal("lowkey", Assert.IsType<Collection>(ok.Value).Title);
            Assert.Null(repo.GetCollection("lowkey", "NIKI"));
            Assert.Equal(countBefore - 1, repo.GetAllCollections().Count());
        }

        [Fact]
        public void Delete_Returnerer404NaarNummeretIkkeFindes()
        {
            var controller = new CollectionController(new CollectionRepositoryList());

            var response = controller.Delete("Findes ikke", "Ingen");

            Assert.IsType<NotFoundResult>(response.Result);
        }
    }
}
