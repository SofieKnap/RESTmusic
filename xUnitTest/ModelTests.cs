using RESTmusic;
using RESTmusic.Controllers;
using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    public class ModelTests
    {
        [Fact]
        public void Collection_ToStringIndeholderTitelKunstnerOgAar()
        {
            // Arrange
            var album = new Collection { Title = "lowkey", Artist = "NIKI", Duration = 185, PublicationYear = 2019 };

            // Act
            string result = album.ToString();

            // Assert
            Assert.Equal("lowkey - NIKI (2019)", result);
        }

        [Fact]
        public void WeatherForecast_GetReturnererFemPrognoserMedGyldigeOplysninger()
        {
            // Arrange: Dette er standardeksemplet fra ASP.NET, ikke musikfunktionerne.
            var controller = new WeatherForecastController();
            DateOnly before = DateOnly.FromDateTime(DateTime.Now);

            // Act
            var result = controller.Get().ToList();
            DateOnly after = DateOnly.FromDateTime(DateTime.Now);

            // Assert: Datoerne tåler også, at testen krydser midnat.
            Assert.Equal(5, result.Count);
            for (int index = 0; index < result.Count; index++)
            {
                Assert.InRange(result[index].Date, before.AddDays(index + 1), after.AddDays(index + 1));
                Assert.InRange(result[index].TemperatureC, -20, 54);
                Assert.Contains(result[index].Summary, new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" });
            }
        }
    }
}
