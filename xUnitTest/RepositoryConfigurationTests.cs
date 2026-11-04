using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RESTmusic.Models;

namespace xUnitTest;

public class RepositoryConfigurationTests
{
    [Fact]
    public void Database_KanVaelgesOgFaarEtNytRepositoryForHvertRequestScope()
    {
        // Test opsætningen uden at forbinde til en rigtig SQL-server.
        using var factory = OpretApp("Database", "Server=localhost;Database=Test;Integrated Security=True;Encrypt=True");
        using var first = factory.Services.CreateScope();
        using var second = factory.Services.CreateScope();

        var repository = first.ServiceProvider.GetRequiredService<ICollectionRepository>();

        Assert.IsType<CollectionRepositoryDB>(repository);
        Assert.Same(repository, first.ServiceProvider.GetRequiredService<ICollectionRepository>());
        Assert.NotSame(repository, second.ServiceProvider.GetRequiredService<ICollectionRepository>());
        var context = first.ServiceProvider.GetRequiredService<CollectionDbContext>();
        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
    }

    [Fact]
    public void Database_UdenForbindelseGiverEnTydeligFejl()
    {
        using var factory = OpretApp("Database", "");

        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("MusicDatabase", error.Message);
    }

    [Fact]
    public void UkendtRepositoryTypeAfvises()
    {
        using var factory = OpretApp("Databsae", "");

        var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("Repository:Type", error.Message);
    }

    private static WebApplicationFactory<Program> OpretApp(string type, string connection)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Repository:Type", type);
            builder.UseSetting("ConnectionStrings:MusicDatabase", connection);
        });
    }
}
