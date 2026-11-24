using BookRest.Api.Options;
using BookRest.Application.Common.Interfaces.Emailing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace BookRest.Api.Services;

internal sealed class PasswordResetLinkFactory : IPasswordResetLinkFactory
{
    private readonly ClientOptions _clientOptions;

    public PasswordResetLinkFactory(IOptions<ClientOptions> apiOptions)
    {
        _clientOptions = apiOptions.Value;
    }

    public string Create(Guid applicationUserId, string resetToken)
    {
        var baseUrl = _clientOptions.ResetPasswordUrl;

        var uri = QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            ["applicationUserId"] = applicationUserId.ToString(),
            ["resetToken"] = resetToken
        });

        return uri;
    }
}
