using System.Net;
using System.Net.Http.Json;
using BookRest.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class VerifyEmailTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public VerifyEmailTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenValidCredentials_ShouldSendOneVerificationEmail()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        var sentEmail = Assert.Single(_factory.EmailsSentTo(email));

        var expectedPathAndQuery = "/api/v1/auth/verify-email";
        var actualPathAndQuery = sentEmail.GetEmailVerificationLink().PathAndQuery;

        Assert.Contains(expectedPathAndQuery, actualPathAndQuery);
    }

    [Fact]
    public async Task VerifyEmail_WhenEmailVerificationLinkIsValid_ShouldConfirmEmail()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        Assert.False(await _factory.IsEmailConfirmedAsync(email));
        var emailVerificationLink = _factory.EmailsSentTo(email).Last().GetEmailVerificationLink();

        var emailVerificationResponse = await _httpClient.GetAsync(emailVerificationLink.PathAndQuery);

        Assert.Equal(HttpStatusCode.OK, emailVerificationResponse.StatusCode);
        Assert.True(await _factory.IsEmailConfirmedAsync(email));
    }

    [Fact]
    public async Task VerifyEmail_WhenEmailVerificationLinkUsedTwice_ShouldReturnOkTwice()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var emailVerificationLink = _factory.EmailsSentTo(email).Last().GetEmailVerificationLink();

        var firstEmailVerificationResponse = await _httpClient.GetAsync(emailVerificationLink.PathAndQuery);
        var secondEmailVerificationResponse = await _httpClient.GetAsync(emailVerificationLink.PathAndQuery);

        Assert.Equal(HttpStatusCode.OK, firstEmailVerificationResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondEmailVerificationResponse.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WhenEmailConfirmationTokenInvalid_ShouldReturn400BadRequestAndKeepEmailUnconfirmed()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var emailVerificationLink = _factory.EmailsSentTo(email).Last().GetEmailVerificationLink();
        var invalidUri = $"/api/v1/auth/verify-email?applicationUserId={emailVerificationLink.ApplicationUserId}&confirmationToken=not-a-token";

        var emailVerificationResponse = await _httpClient.GetAsync(invalidUri);

        Assert.Equal(HttpStatusCode.BadRequest, emailVerificationResponse.StatusCode);
        var problemDetails = await emailVerificationResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        var expectedErrorCode = "InvalidToken";
        var actualErrorCode = problemDetails!.Errors.Keys;

        Assert.Contains(expectedErrorCode, actualErrorCode);
        Assert.False(await _factory.IsEmailConfirmedAsync(email));
    }

    [Fact]
    public async Task VerifyEmail_WhenTokenBelongsToAnotherUser_ShouldReturn400BadRequestAndKeepEmailUnconfirmed()
    {
        var firstEmail = await _httpClient.RegisterCustomerAsync();
        var secondEmail = await _httpClient.RegisterCustomerAsync();
        var firstEmailVerificationLink = _factory.EmailsSentTo(firstEmail).Last().GetEmailVerificationLink();
        var secondEmailVerificationLink = _factory.EmailsSentTo(secondEmail).Last().GetEmailVerificationLink();
        // Simulate a link forgery: set first email verification token and second applicationUserId
        var uri = $"/api/v1/auth/verify-email?applicationUserId={secondEmailVerificationLink.ApplicationUserId}&confirmationToken={firstEmailVerificationLink.ConfirmationToken}";

        var emailVerificationResponse = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.BadRequest, emailVerificationResponse.StatusCode);
        Assert.False(await _factory.IsEmailConfirmedAsync(secondEmail));
    }

    [Fact]
    public async Task VerifyEmail_WhenUserDoesNotExist_ShouldReturn404NotFound()
    {
        var uri = $"/api/v1/auth/verify-email?applicationUserId={Guid.NewGuid()}&confirmationToken=some-token";

        var emailVerificationResponse = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.NotFound, emailVerificationResponse.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WhenTokenIsEmpty_ShouldReturn400BadRequest()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var emailVerificationLink = _factory.EmailsSentTo(email).Last().GetEmailVerificationLink();
        var uri = $"/api/v1/auth/verify-email?applicationUserId={emailVerificationLink.ApplicationUserId}&confirmationToken=";

        var emailVerificationResponse = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.BadRequest, emailVerificationResponse.StatusCode);
    }
}
