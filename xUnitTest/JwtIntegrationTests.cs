using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RESTmusic.Models;
using Xunit;

namespace xUnitTest
{
    // Testene bruger den rigtige routing, JWT-validering og Admin-kontrol i hukommelsen.
    // Der sendes ingen ændringer til Azure eller jeres kørende lokale API.
    public class JwtIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public JwtIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Get_KanLaeseMusikUdenLogin()
        {
            using var client = OpretClient();

            var response = await client.GetAsync("/api/Collection");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var albums = await response.Content.ReadFromJsonAsync<List<Collection>>();
            Assert.Contains(albums!, album => album.Title == "lowkey" && album.Artist == "NIKI");
        }

        [Fact]
        public async Task Login_ForkertAdgangskodeGiver401()
        {
            using var client = OpretClient();

            var response = await client.PostAsJsonAsync("/api/Auth/login", new { username = "admin", password = "forkert" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public async Task Aendringer_UdenLoginGiver401(string method)
        {
            using var client = OpretClient();
            using var request = OpretAendring(method);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public async Task Aendringer_UdenAdminRolleGiver403(string method)
        {
            using var client = OpretClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpretToken("User"));
            using var request = OpretAendring(method);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("udloebet")]
        [InlineData("forkert-signatur")]
        [InlineData("forkert-udsteder")]
        [InlineData("forkert-modtager")]
        public async Task Delete_AfviserUgyldigtToken(string fejl)
        {
            using var client = OpretClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpretToken("Admin", fejl));

            var response = await client.DeleteAsync("/api/Collection/lowkey/NIKI");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            // Et afvist kald må ikke have slettet nummeret.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/Collection/lowkey/NIKI")).StatusCode);
        }

        [Fact]
        public async Task Login_AdminKanOpretteOpdatereOgSletteMedToken()
        {
            // Arrange: Log ind gennem HTTP, præcis som frontenden gør.
            using var client = OpretClient();
            var login = await client.PostAsJsonAsync("/api/Auth/login", new { username = "admin", password = "1234" });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            string token = json.RootElement.GetProperty("token").GetString()!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var album = new Collection { Title = "JWT-test-" + Guid.NewGuid(), Artist = "Testkunstner", Duration = 180, PublicationYear = 2026 };

            // Act og Assert: Opret, og følg Location-adressen.
            var created = await client.PostAsJsonAsync("/api/Collection", album);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            Assert.NotNull(created.Headers.Location);
            string originalUrl = created.Headers.Location!.ToString();
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(originalUrl)).StatusCode);

            // Opdater også nøglen, og hent nummeret på den nye adresse.
            album.Title += "-opdateret";
            album.Duration = 210;
            Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(originalUrl, album)).StatusCode);
            string updatedUrl = "/api/Collection/" + Uri.EscapeDataString(album.Title) + "/" + Uri.EscapeDataString(album.Artist);
            var updated = await client.GetFromJsonAsync<Collection>(updatedUrl);
            Assert.Equal(210, updated!.Duration);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(originalUrl)).StatusCode);

            // Slet kun det nummer, som testen selv oprettede.
            Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync(updatedUrl)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(updatedUrl)).StatusCode);
        }

        [Fact]
        public async Task Cors_TilladerFrontendAtSendeBearerToken()
        {
            using var client = OpretClient();
            const string origin = "https://frontmusiccollection-fwa7eybkd0f3hzg6.swedencentral-01.azurewebsites.net";
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/Collection/lowkey/NIKI");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "DELETE");
            request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
            Assert.Contains("authorization", string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")), StringComparison.OrdinalIgnoreCase);
        }

        private HttpClient OpretClient()
        {
            return factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });
        }

        private static HttpRequestMessage OpretAendring(string method)
        {
            string url = method == "POST" ? "/api/Collection" : "/api/Collection/lowkey/NIKI";
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (method != "DELETE")
            {
                request.Content = JsonContent.Create(new Collection { Title = "lowkey", Artist = "NIKI", Duration = 185, PublicationYear = 2019 });
            }
            return request;
        }

        private string OpretToken(string role, string? fejl = null)
        {
            var config = factory.Services.GetRequiredService<IConfiguration>();
            string key = fejl == "forkert-signatur" ? "Denne-noegle-bruges-kun-til-et-ugyldigt-testtoken-1234" : config["Jwt:Key"]!;
            var token = new JwtSecurityToken(
                issuer: fejl == "forkert-udsteder" ? "Forkert" : config["Jwt:Issuer"],
                audience: fejl == "forkert-modtager" ? "Forkert" : config["Jwt:Audience"],
                claims: new[] { new Claim(ClaimTypes.Name, "testbruger"), new Claim(ClaimTypes.Role, role) },
                notBefore: DateTime.UtcNow.AddHours(-1),
                expires: fejl == "udloebet" ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddMinutes(5),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
