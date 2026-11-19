
namespace BookRest.Api.IntegrationTests.Common.Models.Emailing;

/// <summary>
/// Represents an email that was sent to a user.
/// </summary>
public sealed record SentEmail(
    string ToAddress,
    string Subject,
    string Body);
