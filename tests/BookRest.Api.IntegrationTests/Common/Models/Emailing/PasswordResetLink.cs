namespace BookRest.Api.IntegrationTests.Common.Models.Emailing;

/// <summary>
/// Represents a password reset link.
/// Acts like a container for the three pieces of information from the link in an email, so the tests don’t parse text themselves
/// </summary>
public sealed record PasswordResetLink(
    string PageUrl,
    Guid ApplicationUserId,
    string ResetToken);
