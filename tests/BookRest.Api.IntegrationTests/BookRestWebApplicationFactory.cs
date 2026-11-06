using BookRest.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BookRest.Api.IntegrationTests;

public class BookRestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Specify the environment to be used by the web host
        builder.UseEnvironment("Testing");

        // Overriding the appsettings.json connection string
        builder.UseSetting("ConnectionStrings:BookRestDb", _databaseContainer.GetConnectionString());

        // Overriding the appsettings.json jwt options
        builder.UseSetting("JwtOptions:SecretKey", "test-secret-key-that-is-at-least-32-chars-long");
        builder.UseSetting("JwtOptions:Issuer", "test-issuer");
        builder.UseSetting("JwtOptions:Audience", "test-audience");
    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        using var scope = Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _databaseContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
