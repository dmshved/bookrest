using System.Net;
using BookRest.Api.IntegrationTests.Infrastructure;
using BookRest.Api.Options;
using BookRest.Application.Common.Errors;
using BookRest.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class ForgotPasswordTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public ForgotPasswordTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailWasSent_ShouldReturnOkAndSendOneEmail()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        var forgotPasswordResponse = await _httpClient.PostForgotPasswordAsync(email);

        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);
        Assert.Single(_factory.PasswordResetEmailsSentTo(email));
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailWasSent_ShouldSendLinkToClientPage()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var expectedPasswordResetPage = _factory.Services.GetRequiredService<IOptions<ClientOptions>>().Value.ResetPasswordUrl;

        await _httpClient.PostForgotPasswordAsync(email);

        var resetLink = _factory.PasswordResetEmailsSentTo(email).Single().GetPasswordResetLink();
        Assert.Equal(expectedPasswordResetPage, resetLink.PageUrl);
        Assert.NotEqual(Guid.Empty, resetLink.ApplicationUserId);
        Assert.False(string.IsNullOrWhiteSpace(resetLink.ResetToken));
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailIsUppercase_ShouldSendEmail()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        var forgotPasswordResponse = await _httpClient.PostForgotPasswordAsync(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);
        Assert.Single(_factory.PasswordResetEmailsSentTo(email));
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailIsNotPersisted_ShouldReturnOkAndSendNothing()
    {
        var email = $"{Guid.NewGuid()}@gmail.com";

        var forgotPasswordResponse = await _httpClient.PostForgotPasswordAsync(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);
        Assert.Empty(_factory.PasswordResetEmailsSentTo(email));
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailIsKnownOrUnknown_ShouldReturnOk()
    {
        var knownEmail = await _httpClient.RegisterCustomerAsync();
        var unknownEmail = $"{Guid.NewGuid()}@gmail.com";

        var knownEmailResponse = await _httpClient.PostForgotPasswordAsync(knownEmail);
        var unknownEmailResponse = await _httpClient.PostForgotPasswordAsync(unknownEmail);

        Assert.Equal(HttpStatusCode.OK, knownEmailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unknownEmailResponse.StatusCode);

        var knownResponseBody = await knownEmailResponse.Content.ReadAsStringAsync();
        var unknownResponseBody = await unknownEmailResponse.Content.ReadAsStringAsync();

        Assert.Equal(knownResponseBody, unknownResponseBody);
    }

    [Theory]
    [InlineData("")] // Empty email address
    [InlineData("not-an-email")] // Not an email address
    public async Task ForgotPassword_WhenEmailIsInvalid_ShouldReturn400BadRequest(string invalidEmail)
    {
        var forgotPasswordResponse = await _httpClient.PostForgotPasswordAsync(invalidEmail);

        Assert.Equal(HttpStatusCode.BadRequest, forgotPasswordResponse.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailDeliveryFails_ShouldStillReturnOk()
    {
        var email = await _httpClient.RegisterCustomerAsync();

        _factory.EmailServiceMock
            .Setup(service => service.SendEmailAsync(
                toAddress: It.IsAny<string>(),
                subject: It.IsAny<string>(),
                body: It.IsAny<string>(),
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(EmailDeliveryErrors.SendFailed()));

        try
        {
            var forgotPasswordResponse = await _httpClient.PostForgotPasswordAsync(email);
            Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);
        }
        finally
        {
            // Since the mock is shared we should always restore it
            _factory.ResetEmailServiceMock();
        }
    }
}
