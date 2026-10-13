using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Infrastructure.Data.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace BookRest.Infrastructure.Identity.Services;

public class JwtTokenProvider : ITokenProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly IClock _clock;
    private readonly string _securityAlgorithm;
    private readonly JsonWebTokenHandler _tokenHandler;

    public JwtTokenProvider(
        IOptions<JwtOptions> jwtOptions,
        IClock clock)
    {
        _jwtOptions = jwtOptions.Value;
        _clock = clock;
        _securityAlgorithm = SecurityAlgorithms.HmacSha256;
        // Learn more at https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/8/securitytoken-events?view=aspnetcore-10.0
        _tokenHandler = new JsonWebTokenHandler();
    }

    public (string AccessToken, Instant ExpiresAt) GenerateAccessToken(ApplicationUserInfo user, IEnumerable<string> roles)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, _securityAlgorithm);
        var now = _clock.GetCurrentInstant();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.ApplicationUserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now.ToDateTimeUtc()).ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            new("application_user_id", user.ApplicationUserId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.Plus(Duration.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes)).ToDateTimeUtc(),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials,
            TokenType = "at+jwt" // OAuth 2.0 access token type (indicates that the content is a JWT access token)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var expiresAt = now.Plus(Duration.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));

        return (token, expiresAt);
    }

    public (string RefreshToken, Instant ExpiresAt) GenerateRefreshToken()
    {
        var now = _clock.GetCurrentInstant();

        var randomNumber = new byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();

        randomNumberGenerator.GetBytes(randomNumber);

        var token = Convert.ToBase64String(randomNumber);
        var expiresAt = now.Plus(Duration.FromDays(_jwtOptions.RefreshTokenExpirationDays));

        return (token, expiresAt);
    }

    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),

            // We want to read expired tokens for refresh.
            ValidateLifetime = false,

            RequireExpirationTime = true,

            RequireSignedTokens = true,

            ClockSkew = Duration.FromMinutes(1).ToTimeSpan(),

            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };

        try
        {
            var result = await _tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

            if (!result.IsValid)
            {
                return null;
            }

            if (result.SecurityToken is not JsonWebToken jsonWebToken ||
                !string.Equals(jsonWebToken.Alg, _securityAlgorithm, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }
        catch
        {
            return null;
        }
    }
}
