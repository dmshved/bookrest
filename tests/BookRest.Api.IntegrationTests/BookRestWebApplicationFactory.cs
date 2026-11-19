using System.Collections.Concurrent;
using BookRest.Api.IntegrationTests.Common.Models.Emailing;
using BookRest.Application.Common.Interfaces.Emailing;
using BookRest.Domain.Shared;
using BookRest.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
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

    // Use ConcurrentQueue because a plain List<T> isn't safe when one thread writes while another reads, it can corrupt its internal state or throw
    private readonly ConcurrentQueue<SentEmail> _sentEmails = new();

    public Mock<IEmailService> EmailServiceMock { get; } = new();

    // The collection grows over the whole run, nothing empties concurrent queue between tests, all tests share one factory so emails from earlier tests are persisted
    public IReadOnlyCollection<SentEmail> SentEmails => _sentEmails;

    public BookRestWebApplicationFactory()
    {
        SetupEmailServiceMock();
    }

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

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(EmailServiceMock.Object);
        });
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

    public void ResetEmailServiceMock()
    {
        EmailServiceMock.Reset();
        SetupEmailServiceMock();
    }

    private void SetupEmailServiceMock()
    {
        EmailServiceMock
            .Setup(service => service.SendEmailAsync(
                toAddress: It.IsAny<string>(),
                subject: It.IsAny<string>(),
                body: It.IsAny<string>(),
                cancellationToken: It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((toAddress, subject, body, _) =>
            {
                var email = new SentEmail(toAddress, subject, body);
                _sentEmails.Enqueue(email);
            }).ReturnsAsync(Result.Success);
    }
}
