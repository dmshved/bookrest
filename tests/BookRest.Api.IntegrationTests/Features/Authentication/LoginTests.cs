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
public class LoginTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public LoginTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ShouldReturnOkAndJwtTokens()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var loginResponse = await _httpClient.PostLoginAsync(email);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessTokenExpiresAt));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshTokenExpiresAt));
        Assert.NotEqual(Guid.Empty, authResponse.ApplicationUserId);
    }

    [Theory]
    [InlineData(Roles.Administrator)]
    [InlineData(Roles.Customer)]
    [InlineData(Roles.Owner)]
    public async Task Login_WhenValidCredentials_ShouldReturnAccessTokenWithRole(string roleName)
    {
        var email = await _factory.CreateUserWithRole(roleName);
        var authResponse = await _httpClient.LoginAsync(email);
        var accessToken = new JsonWebTokenHandler().ReadJsonWebToken(authResponse.AccessToken);

        Assert.Equal(authResponse.ApplicationUserId.ToString(), accessToken.Subject);
        var roleClaim = Assert.Single(accessToken.Claims, claim => claim.Type == "role");
        Assert.Equal(roleName, roleClaim.Value);
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ShouldStoreRefreshToken()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var authResponse = await _httpClient.LoginAsync(email);
        var refreshToken = authResponse.RefreshToken;

        using var scope = _factory.Services.CreateScope();
        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        var storedRefreshToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        Assert.NotNull(storedRefreshToken);
        Assert.Equal(authResponse.ApplicationUserId, storedRefreshToken.ApplicationUserId);
        Assert.False(storedRefreshToken.IsRevoked);
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ShouldReturn401Unauthorized()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var loginResponse = await _httpClient.PostLoginAsync(email, password: "invalid-password");

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_ShouldReturn401Unauthorized()
    {
        var loginResponse = await _httpClient.PostLoginAsync(email: "nonexistent@gmail.com", password: "invalid-password");

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", "Password1!")]
    [InlineData("invalid-email", "Password1!")]
    [InlineData("tester@gmail.com", "")]
    [InlineData("tester@gmail.com", "short")]
    public async Task Login_WhenInvalidRequest_ShouldReturn400BadRequest(string email, string password)
    {
        var loginResponse = await _httpClient.PostLoginAsync(email, password);

        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
    }
}
