using System.ComponentModel.DataAnnotations;
using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    public class CollectionValidationTests
    {
        // [ApiController] bruger disse valideringsregler før POST og PUT køres.
        // Her tester vi reglerne direkte, uden en webserver         // theory bruges når samme test køres med flere sæt testdata.

        [Theory]
        [InlineData(null, "Kunstner", 180, 2026, "Title")]
        [InlineData("", "Kunstner", 180, 2026, "Title")]
        [InlineData(" ", "Kunstner", 180, 2026, "Title")]
        [InlineData("Titel", null, 180, 2026, "Artist")]
        [InlineData("Titel", "", 180, 2026, "Artist")]
        [InlineData("Titel", " ", 180, 2026, "Artist")]
        [InlineData("Titel", "Kunstner", 0, 2026, "Duration")]
        [InlineData("Titel", "Kunstner", -1, 2026, "Duration")]
        [InlineData("Titel", "Kunstner", 180, 0, "PublicationYear")]
        [InlineData("Titel", "Kunstner", 180, 10000, "PublicationYear")]
        public void Collection_AfviserUgyldigeOplysninger(
            string? title, string? artist, int duration, int year, string forventetFelt)
        {
            // Arrange: Null bruges bevidst som ugyldigt input i testen.
            var album = new Collection { Title = title!, Artist = artist!, Duration = duration, PublicationYear = year };
            var fejl = new List<ValidationResult>();

            // Act: true betyder, at alle valideringsregler skal kontrolleres.
            bool gyldig = Validator.TryValidateObject(album, new ValidationContext(album), fejl, true);

            // Assert: Fejlen skal pege på det rigtige felt.
            Assert.False(gyldig);
            Assert.Contains(fejl, fejlbesked => fejlbesked.MemberNames.Contains(forventetFelt));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(int.MaxValue, 9999)]
        public void Collection_AcceptererVaerdierPaaDeTilladteGraenser(int duration, int year)
        {
            // Arrange
            var album = new Collection { Title = "Titel", Artist = "Kunstner", Duration = duration, PublicationYear = year };
            var fejl = new List<ValidationResult>();

            // Act
            bool gyldig = Validator.TryValidateObject(album, new ValidationContext(album), fejl, true);

            // Assert
            Assert.True(gyldig);
            Assert.Empty(fejl);
        }
    }
}
