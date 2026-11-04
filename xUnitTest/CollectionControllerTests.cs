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

            var response = controller.GetCollection(1);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            var album = Assert.IsType<Collection>(ok.Value);
            Assert.Equal("lowkey", album.Title);
            Assert.Equal("NIKI", album.Artist);
        }

        [Fact]
        public void GetCollection_Returnerer404ForUkendtId()
        {
            var controller = new CollectionController(new CollectionRepositoryList());

            var response = controller.GetCollection(int.MaxValue);

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
            var saved = Assert.IsType<Collection>(created.Value);
            Assert.True(saved.Id > 0);
            Assert.Equal(saved.Id, created.RouteValues!["id"]);
            Assert.Equal(album.Title, saved.Title);
            Assert.Same(saved, repo.GetCollection(saved.Id));
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
        public void Put_OpdatererAlleOplysningerOgBevarerId()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            var updated = new Collection { Title = "Ny titel", Artist = "Ny kunstner", Duration = 200, PublicationYear = 2026 };

            var response = controller.Put(1, updated);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            var album = Assert.IsType<Collection>(ok.Value);
            Assert.Equal(updated.Title, album.Title);
            Assert.Equal(updated.Artist, album.Artist);
            Assert.Equal(updated.Duration, album.Duration);
            Assert.Equal(updated.PublicationYear, album.PublicationYear);
            Assert.Equal(1, album.Id);
            Assert.Same(album, repo.GetCollection(1));
        }

        [Fact]
        public void Put_KanBeholdeTitelOgKunstner()
        {
            var controller = new CollectionController(new CollectionRepositoryList());
            var updated = new Collection { Title = "lowkey", Artist = "NIKI", Duration = 200, PublicationYear = 2019 };

            var response = controller.Put(1, updated);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(200, Assert.IsType<Collection>(ok.Value).Duration);
        }

        [Fact]
        public void Put_Returnerer404NaarNummeretIkkeFindes()
        {
            var controller = new CollectionController(new CollectionRepositoryList());
            var updated = new Collection { Title = "Ny titel", Artist = "Ny kunstner", Duration = 200, PublicationYear = 2026 };

            var response = controller.Put(int.MaxValue, updated);

            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public void Put_Returnerer409UdenAtAendreDetOprindeligeNummer()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            var updated = new Collection { Title = "Gabriela", Artist = "KATSEYE", Duration = 1, PublicationYear = 2026 };

            var response = controller.Put(1, updated);

            Assert.IsType<ConflictObjectResult>(response.Result);
            Assert.Equal(185, repo.GetCollection(1)!.Duration);
            Assert.Equal(192, repo.GetCollection(3)!.Duration);
        }

        // Disse tests kalder metoden direkte. Login og Admin-rollen håndhæves af HTTP-pipelinen.
        [Fact]
        public void Delete_FjernerNummeretOgReturnererDet()
        {
            var repo = new CollectionRepositoryList();
            var controller = new CollectionController(repo);
            int countBefore = repo.GetAllCollections().Count();

            var response = controller.Delete(1);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal("lowkey", Assert.IsType<Collection>(ok.Value).Title);
            Assert.Null(repo.GetCollection(1));
            Assert.Equal(countBefore - 1, repo.GetAllCollections().Count());
        }

        [Fact]
        public void Delete_Returnerer404NaarNummeretIkkeFindes()
        {
            var controller = new CollectionController(new CollectionRepositoryList());

            var response = controller.Delete(int.MaxValue);

            Assert.IsType<NotFoundResult>(response.Result);
        }
    }
}
