using BookRest.Api.Constants;
using BookRest.Api.Options;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Emailing;
using Microsoft.Extensions.Options;

namespace BookRest.Api.Services;

internal sealed class EmailVerificationLinkFactory : IEmailVerificationLinkFactory
{
    private readonly LinkGenerator _linkGenerator;
    private readonly ApiOptions _apiOptions;

    public EmailVerificationLinkFactory(
        LinkGenerator linkGenerator,
        IOptions<ApiOptions> apiOptions)
    {
        _linkGenerator = linkGenerator;
        _apiOptions = apiOptions.Value;
    }

    public string Create(Guid applicationUserId, string confirmationToken)
    {
        var path = _linkGenerator.GetPathByName(
            endpointName: AuthRouteNames.VerifyEmail,
            values: new { v = "1", userId = applicationUserId, confirmationToken });

        if (path == null)
        {
            throw new InvalidOperationException("Failed to create the email verification link.");
        }

        var uri = new Uri(new Uri(_apiOptions.PubliBaseUrl), path).ToString();

        return uri;
    }
}
