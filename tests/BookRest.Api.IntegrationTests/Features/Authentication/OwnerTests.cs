using System.Net;
using System.Net.Http.Json;
using BookRest.Api.IntegrationTests.Common;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class OwnerTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public OwnerTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldReturnOkAndJwtTokens()
    {
        var registerResponse = await _httpClient.PostRegisterOwnerAsync();

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessTokenExpiresAt));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshTokenExpiresAt));
        Assert.NotEqual(Guid.Empty, authResponse.ApplicationUserId);
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldReturnAccessTokenWithCustomerRole()
    {
        var registerResponse = await _httpClient.PostRegisterOwnerAsync();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        Assert.NotNull(authResponse);

        var accessToken = new JsonWebTokenHandler().ReadJsonWebToken(authResponse.AccessToken);

        Assert.Equal(authResponse.ApplicationUserId.ToString(), accessToken.Subject);
        var roleClaim = Assert.Single(accessToken.Claims, claim => claim.Type == "role");
        Assert.Equal(Roles.Owner, roleClaim.Value);
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldStoreRefreshToken()
    {
        var registerResponse = await _httpClient.PostRegisterOwnerAsync();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        Assert.NotNull(authResponse);
        var refreshToken = authResponse.RefreshToken;

        using var scope = _factory.Services.CreateScope();
        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        var storedRefreshToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        Assert.NotNull(storedRefreshToken);
        Assert.Equal(authResponse.ApplicationUserId, storedRefreshToken.ApplicationUserId);
        Assert.False(storedRefreshToken.IsRevoked);
    }

    [Fact]
    public async Task Register_WhenEmailIsWrong_ShouldReturn400BadRequest()
    {
        var email = "invalid-email";

        var registerResponse = await _httpClient.PostRegisterOwnerAsync(email);

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenPasswordIsWrong_ShouldReturn400BadRequest()
    {
        var registerResponse = await _httpClient.PostRegisterOwnerAsync(password: "invalid-password");

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenOwnerAlreadyExists_ShouldReturn400BadRequest()
    {
        var email = $"{Guid.NewGuid()}@gmail.com";

        var firstRegisterResponse = await _httpClient.PostRegisterOwnerAsync(email);
        var secondRegisterResponse = await _httpClient.PostRegisterOwnerAsync(email);

        Assert.Equal(HttpStatusCode.OK, firstRegisterResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondRegisterResponse.StatusCode);
    }
}
