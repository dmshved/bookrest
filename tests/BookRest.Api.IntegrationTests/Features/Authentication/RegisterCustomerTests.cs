using System.Net;
using System.Net.Http.Json;
using BookRest.Api.IntegrationTests.Infrastructure;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class RegisterCustomerTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public RegisterCustomerTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldReturnOkAndJwtTokens()
    {
        var registerResponse = await _httpClient.PostRegisterCustomerAsync();

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessTokenExpiresAt));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshTokenExpiresAt));
        Assert.NotEqual(Guid.Empty, authResponse.ApplicationUserId);
    }

    [Theory]
    [InlineData("", "customer@gmail.com", "Password1!", "Password1!")] // Name is empty
    [InlineData("x", "customer@gmail.com", "Password1!", "Password1!")] // Name too short
    [InlineData("customer-name", "customer@gmail.com", "Pass1!", "Pass1!")] // Password too short
    [InlineData("customer-name", "customer@gmail.com", "Password1!", "OtherPassword1!")] // Confirmation mismatch
    public async Task Register_WhenInvalidCredentials_ShouldReturn400BadRequest(string userName, string email, string password, string confirmPassword)
    {
        var registerResponse = await _httpClient.PostRegisterCustomerAsync(userName, email, password, confirmPassword);

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldCreateCustomerProfile()
    {
        var email = $"{Guid.NewGuid()}@gmail.com";
        var userName = $"customer-{Guid.NewGuid()}";

        var registerResponse = await _httpClient.PostRegisterCustomerAsync(email: email, userName: userName);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();

        using var scope = _factory.Services.CreateScope();
        var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var storedCustomer = await customerRepository.GetByApplicationUserIdAsync(authResponse!.ApplicationUserId);

        Assert.NotNull(storedCustomer);
        Assert.Equal(userName, storedCustomer.UserName);
        Assert.Equal(email.ToUpperInvariant(), storedCustomer.Email.Value);
    }

    [Fact]
    public async Task Register_WhenEmailsHaveDifferentCases_ShouldReturn409Conflict()
    {
        var value = Guid.NewGuid().ToString();

        var firstRegisterResponse = await _httpClient.PostRegisterCustomerAsync(email: $"{value}@gmail.com");
        var secondRegisterResponse = await _httpClient.PostRegisterCustomerAsync(email: $"{value.ToUpperInvariant()}@GMAIL.COM");

        Assert.Equal(HttpStatusCode.OK, firstRegisterResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondRegisterResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldAllowLogin()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        var loginResponse = await _httpClient.PostLoginAsync(email);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldReturnAccessTokenWithCustomerRole()
    {
        var registerResponse = await _httpClient.PostRegisterCustomerAsync();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        Assert.NotNull(authResponse);

        var accessToken = new JsonWebTokenHandler().ReadJsonWebToken(authResponse.AccessToken);

        Assert.Equal(authResponse.ApplicationUserId.ToString(), accessToken.Subject);
        var roleClaim = Assert.Single(accessToken.Claims, claim => claim.Type == "role");
        Assert.Equal(Roles.Customer, roleClaim.Value);
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldStoreRefreshToken()
    {
        var registerResponse = await _httpClient.PostRegisterCustomerAsync();
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

        var registerResponse = await _httpClient.PostRegisterCustomerAsync(email: email);

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenPasswordIsViolatingIdentityRules_ShouldReturn400BadRequest()
    {
        var registerResponse = await _httpClient.PostRegisterCustomerAsync(password: "invalid-password");

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WhenCustomerAlreadyExists_ShouldReturn409Conflict()
    {
        var email = $"{Guid.NewGuid()}@gmail.com";

        var firstRegisterResponse = await _httpClient.PostRegisterCustomerAsync(email: email);
        var secondRegisterResponse = await _httpClient.PostRegisterCustomerAsync(email: email);

        Assert.Equal(HttpStatusCode.OK, firstRegisterResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondRegisterResponse.StatusCode);
    }
}
