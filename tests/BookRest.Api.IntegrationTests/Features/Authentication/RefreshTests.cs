using System.Net;
using System.Net.Http.Json;
using BookRest.Api.IntegrationTests.Common;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Constants;
using BookRest.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using NodaTime;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class RefreshTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public RefreshTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenValidRefreshToken_ShouldReturnOkAndJwtTokens()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var firstAuth = await _httpClient.LoginAsync(email);
        var response = await _httpClient.PostRefreshAsync(firstAuth.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var secondAuth = await response.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        Assert.NotNull(secondAuth);
        Assert.NotEqual(firstAuth.RefreshToken, secondAuth.RefreshToken);
        Assert.NotEqual(firstAuth.AccessToken, secondAuth.AccessToken);
        Assert.Equal(firstAuth.ApplicationUserId, secondAuth.ApplicationUserId);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenValidRefreshToken_ShouldReturnAccessTokenForTheCustomerRole()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var firstAuth = await _httpClient.LoginAsync(email);
        var secondAuth = await _httpClient.RefreshAsync(firstAuth.RefreshToken);

        var accessToken = new JsonWebTokenHandler().ReadJsonWebToken(secondAuth.AccessToken);

        Assert.Equal(firstAuth.ApplicationUserId.ToString(), accessToken.Subject);
        Assert.Contains(accessToken.Claims, claim => claim.Type == "role" && claim.Value == Roles.Customer);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenValidRefreshToken_ShouldRotateTokens()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var firstAuth = await _httpClient.LoginAsync(email);
        var secondAuth = await _httpClient.RefreshAsync(firstAuth.RefreshToken);

        var oldToken = await GetStoredRefreshTokenAsync(firstAuth.RefreshToken);
        var newToken = await GetStoredRefreshTokenAsync(secondAuth.RefreshToken);

        // Check old refresh token
        Assert.NotNull(oldToken);
        Assert.True(oldToken.IsRevoked);
        Assert.Equal(secondAuth.RefreshToken, oldToken.ReplacedByToken);
        // Check new refresh token
        Assert.NotNull(newToken);
        Assert.False(newToken.IsRevoked);
        Assert.Equal(firstAuth.ApplicationUserId, newToken.ApplicationUserId);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenRefreshingNewRefreshToken_ShouldRotateTokens()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        var firstAuth = await _httpClient.LoginAsync(email);
        var secondAuth = await _httpClient.RefreshAsync(firstAuth.RefreshToken);
        var thirdAuth = await _httpClient.RefreshAsync(secondAuth.RefreshToken);

        Assert.NotEqual(secondAuth.RefreshToken, thirdAuth.RefreshToken);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenRefreshTokenDoesNotExist_ShouldReturn401Unauthorized()
    {
        var response = await _httpClient.PostRefreshAsync("invalid-token");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenRefreshTokenIsReused_ShouldReturn401UnauthorizedAndRevokeNewRefreshToken()
    {
        // Register customer
        var email = await _httpClient.RegisterCustomerAsync();
        // Login customer
        var firstAuth = await _httpClient.LoginAsync(email);
        // Refresh token
        var secondAuth = await _httpClient.RefreshAsync(firstAuth.RefreshToken);

        // Reuse the refresh token
        var firstResponse = await _httpClient.PostRefreshAsync(firstAuth.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, firstResponse.StatusCode);

        // New token should be revoked
        var secondToken = await GetStoredRefreshTokenAsync(secondAuth.RefreshToken);

        Assert.NotNull(secondToken);
        Assert.True(secondToken.IsRevoked);

        // Use second refresh token that was revoked too
        var secondResponse = await _httpClient.PostRefreshAsync(secondAuth.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenRefreshTokenIsRevoked_ShouldReturn401Unauthorized()
    {
        // Register customer
        var email = await _httpClient.RegisterCustomerAsync();
        // Login customer
        var authResponse = await _httpClient.LoginAsync(email);
        // Refresh token
        var revokeResponse = await _httpClient.PostRevokeAsync(authResponse.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);

        var refreshResponse = await _httpClient.PostRefreshAsync(authResponse.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerRefreshToken_WhenRefreshTokenIsExpired_ShouldReturn401Unauthorized()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var authResponse = await _httpClient.LoginAsync(email);
        var expiredTokenValue = $"expired-token-{Guid.NewGuid()}";

        // Save expired token in the database
        using (var scope = _factory.Services.CreateScope())
        {
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredRefreshToken = RefreshToken.Create(
                token: expiredTokenValue,
                expiresAt: clock.GetCurrentInstant() - Duration.FromDays(1),
                applicationUserId: authResponse.ApplicationUserId);

            refreshTokenRepository.Add(expiredRefreshToken);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        var refreshResponse = await _httpClient.PostRefreshAsync(expiredTokenValue);

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CustomerRefreshToken_WhenRefreshTokenIsEmpty_ShouldReturn400BadRequest(string refreshTokenValue)
    {
        var refreshResponse = await _httpClient.PostRefreshAsync(refreshTokenValue);

        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);
    }

    private async Task<RefreshToken?> GetStoredRefreshTokenAsync(string refreshTokenValue)
    {
        using var scope = _factory.Services.CreateScope();
        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        return await refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
    }
}
