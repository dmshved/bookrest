namespace BookRest.Api.IntegrationTests.Common.Models.Emailing;

/// <summary>
/// Represents an email verification link.
/// Acts like a container for the three pieces of information from the link in an email, so the tests don’t parse text themselves
/// </summary>
public record EmailVerificationLink(
    string PathAndQuery,
    Guid ApplicationUserId,
    string ConfirmationToken);
