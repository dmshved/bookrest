using BookRest.Api.IntegrationTests.Common;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class LoginTests
{
    private readonly HttpClient _httpClient;

    public LoginTests(BookRestWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ShouldReturnAuthenticationResponseWithJwtTokens()
    {
        var authenticationResponse = await _httpClient.RegisterCustomerAsync();

        Assert.NotNull(authenticationResponse);
        Assert.False(string.IsNullOrWhiteSpace(authenticationResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authenticationResponse.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(authenticationResponse.AccessTokenExpiresAt));
        Assert.False(string.IsNullOrWhiteSpace(authenticationResponse.RefreshTokenExpiresAt));
        Assert.NotEmpty(authenticationResponse.ApplicationUserId.ToString());
    }
}
