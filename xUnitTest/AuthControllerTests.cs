using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RESTmusic.Controllers;
using Xunit;

namespace xUnitTest
{
    public class AuthControllerTests
    {
        // Denne nøgle bruges kun i testene, aldrig i den publicerede app.
        private const string TestKey = "Kun-til-tests-ikke-en-produktionsnoegle-1234567890";

        // theory bruges når samme test køres med flere sæt testdata.
        [Theory]
        [InlineData("admin", "forkert")]
        [InlineData("forkert", "1234")]
        [InlineData("", "")]
        public void Login_ForkerteOplysningerGiver401(string username, string password)
        {
            // Arrange
            var controller = OpretController();
            var login = new LoginRequest { Username = username, Password = password };

            // Act
            var result = controller.Login(login);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public void Login_KorrekteOplysningerGiverGyldigtAdminToken()
        {
            // Arrange: Controlleren får en testkonfiguration i stedet for rigtige nøgler.
            var controller = OpretController();
            var login = new LoginRequest { Username = "admin", Password = "1234" };
            DateTime before = DateTime.UtcNow;

            // Act
            var result = controller.Login(login);
            string token = HentToken(result);

            // Assert: Kontroller signatur, udsteder, modtager og udløbstid.
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
                ValidateIssuer = true,
                ValidIssuer = "RESTmusic.Tests",
                ValidateAudience = true,
                ValidAudience = "FrontMusic.Tests",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            ClaimsPrincipal user = handler.ValidateToken(token, parameters, out var validatedToken);
            var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);
            Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
            Assert.Equal("admin", user.Identity!.Name);
            Assert.True(user.IsInRole("Admin"));
            Assert.Equal("admin", jwt.Subject);
            Assert.InRange(jwt.ValidTo, before.AddHours(2).AddSeconds(-2), DateTime.UtcNow.AddHours(2).AddSeconds(2));
        }

        [Fact]
        public void Login_TwoLoginsGiverForskelligeTokenId()
        {
            // Arrange
            var controller = OpretController();
            var login = new LoginRequest { Username = "admin", Password = "1234" };
            var handler = new JwtSecurityTokenHandler();

            // Act
            var first = handler.ReadJwtToken(HentToken(controller.Login(login)));
            var second = handler.ReadJwtToken(HentToken(controller.Login(login)));

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(first.Id));
            Assert.NotEqual(first.Id, second.Id);
        }

        private static AuthController OpretController()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = TestKey,
                ["Jwt:Issuer"] = "RESTmusic.Tests",
                ["Jwt:Audience"] = "FrontMusic.Tests"
            }).Build();
            return new AuthController(config);
        }

        private static string HentToken(IActionResult result)
        {
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.SerializeToElement(ok.Value);
            string? token = json.GetProperty("token").GetString();
            Assert.False(string.IsNullOrWhiteSpace(token));
            return token!;
        }
    }
}
