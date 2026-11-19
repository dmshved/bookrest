using System.Net;
using BookRest.Api.IntegrationTests.Infrastructure;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class RegistrationTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public RegistrationTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Registration_WhenUserNameIsAlreadyTaken_ShouldReturn()
    {
        var userName = $"customer-{Guid.NewGuid()}";

        var firstRegisterResponse = await _httpClient.PostRegisterCustomerAsync(userName: userName);
        // Register again, the email is new, but the name is the same
        var secondRegisterResponse = await _httpClient.PostRegisterCustomerAsync(userName: userName);

        Assert.Equal(HttpStatusCode.OK, firstRegisterResponse.StatusCode);
        Assert.False(secondRegisterResponse.IsSuccessStatusCode);
    }
}
