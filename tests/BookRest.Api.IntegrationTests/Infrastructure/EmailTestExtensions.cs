using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using BookRest.Api.IntegrationTests.Common.Models.Emailing;
using BookRest.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BookRest.Api.IntegrationTests.Infrastructure;

public static class EmailTestExtensions
{
    private const string RegexLinkPattern = "href='(?<url>[^']+)'";

    /// <summary>
    /// Returns the recorded emails down to the ones addressed to the given address. Comparison is case-insensitive.
    /// </summary>
    public static IEnumerable<SentEmail> EmailsSentTo(this BookRestWebApplicationFactory factory, string address)
    {
        return factory.SentEmails.Where(e => e.ToAddress.Equals(address, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the recorded emails with password reset link. Comparison is case-insensitive.
    /// </summary>
    public static IEnumerable<SentEmail> PasswordResetEmailsSentTo(this BookRestWebApplicationFactory factory, string address)
    {
        return factory.EmailsSentTo(address)
            .Where(e => e.Subject == "Reset your password");
    }

    public static EmailVerificationLink GetEmailVerificationLink(this SentEmail email)
    {
        var match = Regex.Match(email.Body, RegexLinkPattern);

        if (!match.Success)
        {
            throw new InvalidOperationException($"Failed to get link from the email body: {email.Body}");
        }

        // The host part in the uri comes from appsettings.json ApiOptions:PublicBaseUrl, so we need only path and query part
        var uri = new Uri(WebUtility.HtmlDecode(match.Groups["url"].Value));
        var query = HttpUtility.ParseQueryString(uri.Query);

        var applicationUserId = Guid.Parse(query["applicationUserId"]!);
        var confirmationToken = query["confirmationToken"]!;

        return new EmailVerificationLink(
            PathAndQuery: uri.PathAndQuery,
            ApplicationUserId: applicationUserId,
            ConfirmationToken: confirmationToken);
    }

    public static PasswordResetLink GetPasswordResetLink(this SentEmail email)
    {
        var match = Regex.Match(email.Body, RegexLinkPattern);

        if (!match.Success)
        {
            throw new InvalidOperationException($"Failed to get link from the email body: {email.Body}");
        }

        var uri = new Uri(WebUtility.HtmlDecode(match.Groups["url"].Value));
        var query = HttpUtility.ParseQueryString(uri.Query);

        var pageUrl = uri.GetLeftPart(UriPartial.Path);
        var applicationUserId = Guid.Parse(query["applicationUserId"]!);
        var resetToken = query["resetToken"]!;

        return new PasswordResetLink(
            pageUrl,
            applicationUserId,
            resetToken);
    }

    public static async Task<bool> IsEmailConfirmedAsync(this BookRestWebApplicationFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var userInfo = await identityService.GetByEmailAsync(email);

        return userInfo != null && await identityService.IsEmailConfirmedAsync(userInfo.ApplicationUserId);
    }
}
