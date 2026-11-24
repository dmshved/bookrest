using System.Net;
using BookRest.Api.IntegrationTests.Infrastructure;
using BookRest.Application.Common.Errors;
using BookRest.Domain.Shared;
using Moq;

namespace BookRest.Api.IntegrationTests.Features.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class ResendEmailVerificationTests
{
    private readonly BookRestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public ResendEmailVerificationTests(BookRestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Resend_WhenEmailIsNotConfirmed_ShouldSendEmailVerificationLink()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var countEmailsBefore = _factory.EmailsSentTo(email).Count();

        var resendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(email);
        Assert.Equal(HttpStatusCode.OK, resendEmailVerificationResponse.StatusCode);
        var countEmailsAfter = _factory.EmailsSentTo(email).Count();
        Assert.Equal(countEmailsBefore + 1, countEmailsAfter);

        var uri = _factory.EmailsSentTo(email).Last().GetEmailVerificationLink();
        var emailVerificationResponse = await _httpClient.GetAsync(uri.PathAndQuery);
        Assert.Equal(HttpStatusCode.OK, emailVerificationResponse.StatusCode);
        Assert.True(await _factory.IsEmailConfirmedAsync(email));
    }

    [Fact]
    public async Task Resend_WhenEmailIsAlreadyConfirmed_ShouldReturnOkAndSendNothing()
    {
        var email = await _httpClient.RegisterCustomerAsync();
        var uri = _factory.EmailsSentTo(email).Last().GetEmailVerificationLink().PathAndQuery;
        await _httpClient.GetAsync(uri);
        var countEmailsBefore = _factory.EmailsSentTo(email).Count();

        var resendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(email);

        Assert.Equal(HttpStatusCode.OK, resendEmailVerificationResponse.StatusCode);
        var countEmailsAfter = _factory.EmailsSentTo(email).Count();
        Assert.Equal(countEmailsBefore, countEmailsAfter);
    }

    [Fact]
    public async Task Resend_WhenEmailIsNotPersisted_ShouldReturnOkAndSendNothing()
    {
        var unknownEmail = $"{Guid.NewGuid()}@gmail.com";

        var resendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(unknownEmail);

        Assert.Equal(HttpStatusCode.OK, resendEmailVerificationResponse.StatusCode);
        Assert.Empty(_factory.EmailsSentTo(unknownEmail));
    }

    [Fact]
    public async Task Resend_WhenEmailKnownAndUnknown_ShouldReturnOkAndSendNothing()
    {
        var knownEmail = await _httpClient.RegisterCustomerAsync();
        var unknownEmail = $"{Guid.NewGuid()}@gmail.com";

        var knownResendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(knownEmail);
        var unknownResendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(unknownEmail);
        Assert.Equal(knownResendEmailVerificationResponse.StatusCode, unknownResendEmailVerificationResponse.StatusCode);

        var knownResponseContent = await knownResendEmailVerificationResponse.Content.ReadAsStringAsync();
        var unknownResponseContent = await unknownResendEmailVerificationResponse.Content.ReadAsStringAsync();
        Assert.Equal(knownResponseContent, unknownResponseContent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public async Task Resend_WhenInvalidEmail_ShouldReturn400BadRequest(string email)
    {
        var resendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(email);

        Assert.Equal(HttpStatusCode.BadRequest, resendEmailVerificationResponse.StatusCode);
    }

    [Fact]
    public async Task Resend_WhenEmailDeliveryFails_ShouldReturnOk()
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
            var resendEmailVerificationResponse = await _httpClient.PostResendEmailVerificationAsync(email);
            Assert.Equal(HttpStatusCode.OK, resendEmailVerificationResponse.StatusCode);
        }
        finally
        {
            // Since the mock is shared we should always restore it
            _factory.ResetEmailServiceMock();
        }
    }
}
