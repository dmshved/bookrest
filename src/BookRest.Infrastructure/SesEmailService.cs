using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using BookRest.Application.Common.Errors;
using BookRest.Application.Common.Interfaces.Emailing;
using BookRest.Domain.Shared;
using BookRest.Infrastructure.Data.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookRest.Infrastructure;

public partial class SesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailServiceV2 _amazonSes;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<SesEmailService> _logger;

    public SesEmailService(
        IAmazonSimpleEmailServiceV2 amazonSes,
        IOptions<EmailOptions> emailOptions,
        ILogger<SesEmailService> logger)
    {
        _amazonSes = amazonSes;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default)
    {
        // If a display name is configured, combine the name and address. Otherwise use only the email address
        var fromEmailAddress = string.IsNullOrWhiteSpace(_emailOptions.FromDisplayName)
            ? _emailOptions.FromAddress
            : $"{_emailOptions.FromDisplayName} <{_emailOptions.FromAddress}>";

        var destination = new Destination { ToAddresses = [toAddress] };

        var content = new EmailContent
        {
            Simple = new Message
            {
                Subject = new Content { Data = subject },
                Body = new Body { Text = new Content { Data = body } }
            }
        };

        var request = new SendEmailRequest
        {
            FromEmailAddress = fromEmailAddress,
            Destination = destination,
            Content = content
        };

        try
        {
            await _amazonSes.SendEmailAsync(request, cancellationToken);

            return Result.Success();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogException(exception, toAddress);

            return Result.Failure(EmailDeliveryErrors.SendFailed());
        }
    }

    [LoggerMessage(
        LogLevel.Error,
        Message = "AWS SES: Failed to send an email to <{toAddress}>.")]
    private partial void LogException(
        Exception exception,
        string toAddress);
}
