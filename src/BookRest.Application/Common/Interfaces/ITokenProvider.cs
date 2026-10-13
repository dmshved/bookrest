using System.Security.Claims;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Application.Common.Interfaces;

public interface ITokenProvider
{
    (string AccessToken, Instant ExpiresAt) GenerateAccessToken(ApplicationUserInfo user, IEnumerable<string> roles);
    (string RefreshToken, Instant ExpiresAt) GenerateRefreshToken();
    Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token);
}
