using System.Net;
using BookRest.Api.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class RevokeTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public RevokeTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task RevokeToken_WhenValidRefreshToken_ShouldReturnOkAndRevokeRefreshToken()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var authResponse = await _httpClient.LoginAsync(email);
        var refreshTokenValue = authResponse.RefreshToken;
        var expectedReason = "User logout.";
        var clock = _factory.Services.GetRequiredService<IClock>();

        var timeBefore = clock.GetCurrentInstant();
        var response = await _httpClient.PostRevokeAsync(refreshTokenValue);
        var timeAfter = clock.GetCurrentInstant();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var storedRefreshToken = await _factory.GetStoredRefreshTokenAsync(refreshTokenValue);

        Assert.NotNull(storedRefreshToken);
        Assert.True(storedRefreshToken.IsRevoked);
        Assert.Equal(expectedReason, storedRefreshToken.ReasonRevoked);
        Assert.Null(storedRefreshToken.ReplacedByToken);
        Assert.InRange(storedRefreshToken.RevokedAt!.Value, timeBefore, timeAfter);
    }

    [Fact]
    public async Task RevokeToken_WhenValidRefreshToken_ShouldStoreReason()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var authResponse = await _httpClient.LoginAsync(email);
        var refreshToken = authResponse.RefreshToken;
        var reasonRevoked = "Password changed.";

        var revokeResponse = await _httpClient.PostRevokeAsync(refreshToken, reasonRevoked);

        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);
        var storedRefreshToken = await _factory.GetStoredRefreshTokenAsync(refreshToken);
        Assert.Equal(reasonRevoked, storedRefreshToken!.ReasonRevoked);
    }

    [Fact]
    public async Task RevokeToken_WhenRefreshTokenIsRevoked_ShouldReturnOk()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var authResponse = await _httpClient.LoginAsync(email);
        var refreshToken = authResponse.RefreshToken;
        var firstReason = "First reason.";
        var secondReason = "Second reason.";

        var firstRevoke = await _httpClient.PostRevokeAsync(refreshToken, firstReason);
        var firstStoredToken = await _factory.GetStoredRefreshTokenAsync(refreshToken);

        var secondRevoke = await _httpClient.PostRevokeAsync(refreshToken, secondReason);
        var secondStoredToken = await _factory.GetStoredRefreshTokenAsync(refreshToken);

        Assert.Equal(HttpStatusCode.OK, firstRevoke.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondRevoke.StatusCode);
        Assert.Equal(firstStoredToken!.RevokedAt, secondStoredToken!.RevokedAt);
        Assert.Equal(firstReason, secondStoredToken.ReasonRevoked);
    }

    [Fact]
    public async Task RevokeToken_WhenUserHasOtherTokens_ShouldRevokeOnlyGivenToken()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        var firstLogin = await _httpClient.LoginAsync(email);
        var secondLogin = await _httpClient.LoginAsync(email);

        await _httpClient.PostRevokeAsync(firstLogin.RefreshToken);

        var secondRefreshToken = await _factory.GetStoredRefreshTokenAsync(secondLogin.RefreshToken);
        Assert.False(secondRefreshToken!.IsRevoked);

        // Check if second refresh token works
        var refreshResponse = await _httpClient.PostRefreshAsync(secondLogin.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RevokeToken_WhenRefreshTokenIsEmpty_ShouldReturn400BadRequest(string refreshTokenValue)
    {
        var refreshResponse = await _httpClient.PostRevokeAsync(refreshTokenValue);

        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task RevokeToken_WhenRefreshTokenDoesNotExist_ShouldReturnOk()
    {
        var refreshTokenValue = "non-existent-token";

        var response = await _httpClient.PostRevokeAsync(refreshTokenValue);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
