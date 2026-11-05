using System.Security.Claims;
using System.Text;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Infrastructure.Data.Options;
using BookRest.Infrastructure.Identity.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Testing;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace BookRest.Infrastructure.UnitTests.Identity.Services;

public class JwtTokenProviderTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 1, 1, 12, 0);
    private readonly FakeClock _clock = new(Now);

    private readonly JwtOptions _jwtOptions = new()
    {
        SecretKey = "test-secret-key-that-is-at-least-32-chars-long",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    };

    private readonly JwtTokenProvider _tokenProvider;

    private static readonly ApplicationUserInfo ApplicationUserInfo = new(
        ApplicationUserId: Guid.NewGuid(),
        UserName: "user",
        Email: "user@gmail.com");

    public JwtTokenProviderTests()
    {
        _tokenProvider = new JwtTokenProvider(Options.Create(_jwtOptions), _clock);
    }

    #region Access token tests

    // Tests that <see cref="JwtTokenProvider"/> use the current time each time a token is generated.
    [Fact]
    public void GenerateAccessToken_WhenClockAdvances_ShouldUseCurrentInstant()
    {
        _clock.Advance(Duration.FromHours(15));

        var (_, expiresAt) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, []);
        Assert.Equal(_clock.GetCurrentInstant() + Duration.FromMinutes(15), expiresAt);
    }

    [Fact]
    public async Task ValidateAccessToken_WhenClockAdvances_ShouldBeInvalid()
    {
        var (tokenValue, _) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, []);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),

            ValidateLifetime = true,

            RequireExpirationTime = true,

            RequireSignedTokens = true,

            ClockSkew = Duration.FromMinutes(1)
                .ToTimeSpan(),

            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role",

            LifetimeValidator = (_, expires, _, _) => expires > _clock.GetCurrentInstant().ToDateTimeUtc()
        };

        var handler = new JsonWebTokenHandler();

        // The token should be valid after 14 minutes from creation
        _clock.Advance(Duration.FromMinutes(14));
        var beforeExpiry = await handler.ValidateTokenAsync(tokenValue, tokenValidationParameters);
        Assert.True(beforeExpiry.IsValid);

        // Advance time for 2 more minutes so token is expired
        _clock.Advance(Duration.FromMinutes(2));
        var afterExpiry = await handler.ValidateTokenAsync(tokenValue, tokenValidationParameters);
        Assert.False(afterExpiry.IsValid);
    }

    [Fact]
    public void GenerateAccessToken_WhenGenerated_ShouldContainUserClaimsAndRoles()
    {
        var (tokenValue, _) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, ["role1", "role2"]);
        var handler = new JsonWebTokenHandler();
        var accessToken = handler.ReadJsonWebToken(tokenValue);

        Assert.Equal(ApplicationUserInfo.ApplicationUserId.ToString(), accessToken.Subject);
        Assert.Equal(ApplicationUserInfo.Email, accessToken.GetClaim(JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(ApplicationUserInfo.UserName, accessToken.GetClaim(JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(ApplicationUserInfo.ApplicationUserId.ToString(), accessToken.GetClaim("application_user_id").Value);
        Assert.Equal(
            ["role1", "role2"],
            accessToken.Claims.Where(claim => claim.Type == "role")
                .Select(claim => claim.Value).Order());
    }

    [Fact]
    public void GenerateAccessToken_WhenGenerated_ShouldContainHeaderIssuerAndAudience()
    {
        var (tokenValue, _) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, ["role1", "role2"]);
        var handler = new JsonWebTokenHandler();
        var accessToken = handler.ReadJsonWebToken(tokenValue);

        Assert.Equal("HS256", accessToken.Alg);
        Assert.Equal("at+jwt", accessToken.Typ);
        Assert.Equal(_jwtOptions.Issuer, accessToken.Issuer);
        Assert.Contains(_jwtOptions.Audience, accessToken.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_WhenCalledTwice_ShouldGenerateDifferentJti()
    {
        var (firstTokenValue, _) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, []);
        var (secondTokenValue, _) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, []);
        var handler = new JsonWebTokenHandler();
        var firstAccessToken = handler.ReadJsonWebToken(firstTokenValue);
        var secondAccessToken = handler.ReadJsonWebToken(secondTokenValue);

        Assert.NotEqual(firstAccessToken.GetClaim(JwtRegisteredClaimNames.Jti).Value,
            secondAccessToken.GetClaim(JwtRegisteredClaimNames.Jti).Value);
    }

    #endregion

    #region Refresh token tests

    [Fact]
    public void GenerateRefreshToken_WhenGenerated_ShouldReturn64BytesOfValidBase64()
    {
        var (refreshTokenValue, _) = _tokenProvider.GenerateRefreshToken();

        var bytes = Convert.FromBase64String(refreshTokenValue);

        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateRefreshToken_WhenCalledTwice_ShouldReturnDifferentTokens()
    {
        var (firstTokenValue, _) = _tokenProvider.GenerateRefreshToken();
        var (secondTokenValue, _) = _tokenProvider.GenerateRefreshToken();

        Assert.NotEqual(firstTokenValue, secondTokenValue);
    }

    [Fact]
    public void GenerateRefreshToken_WhenClockAdvances_ShouldExpire()
    {
        _clock.Advance(Duration.FromHours(15));

        var (_, expiresAt) = _tokenProvider.GenerateRefreshToken();

        Assert.Equal(_clock.GetCurrentInstant() + Duration.FromDays(7), expiresAt);
    }

    #endregion

    #region Getting principal from access token

    [Fact]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenTokenIsValid_ShouldReturnPrincipal()
    {
        var (accessTokenValue, _) = _tokenProvider.GenerateAccessToken(ApplicationUserInfo, ["role1"]);
        var principal = await _tokenProvider.GetPrincipalFromExpiredTokenAsync(accessTokenValue);

        Assert.NotNull(principal);
        Assert.Equal(ApplicationUserInfo.ApplicationUserId.ToString(), principal.FindFirst("sub")?.Value);
        Assert.Equal(ApplicationUserInfo.UserName, principal.Identity?.Name);
        Assert.True(principal.IsInRole("role1"));
    }

    [Fact]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenTokenIsExpired_ShouldReturnPrincipal()
    {
        var oldClock = new FakeClock(Instant.FromUtc(2020, 1, 1, 0, 0));
        var expiredAccessTokenValue = GenerateAccessTokenWith(_jwtOptions, oldClock);

        var principal = await _tokenProvider.GetPrincipalFromExpiredTokenAsync(expiredAccessTokenValue);

        Assert.NotNull(principal);
    }

    [Fact]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenInvalidSigningKey_ShouldReturnNull()
    {
        var invalidOptions = CopyOptions(secretKey: "a-completely-different-secret-key-32-chars-min");
        var accessTokenValue = GenerateAccessTokenWith(invalidOptions);

        var principal = await _tokenProvider.GetPrincipalFromExpiredTokenAsync(accessTokenValue);

        Assert.Null(principal);
    }

    [Fact]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenWrongIssuer_ShouldReturnNull()
    {
        var invalidOptions = CopyOptions(issuer: "other-issuer");
        var accessTokenValue = GenerateAccessTokenWith(invalidOptions);

        var principal = await _tokenProvider.GetPrincipalFromExpiredTokenAsync(accessTokenValue);

        Assert.Null(principal);
    }

    [Fact]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenWrongAudience_ShouldReturnNull()
    {
        var invalidOptions = CopyOptions(audience: "other-audience");
        var accessTokenValue = GenerateAccessTokenWith(invalidOptions);

        var principal = await _tokenProvider.GetPrincipalFromExpiredTokenAsync(accessTokenValue);

        Assert.Null(principal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a jwt token")]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenTokenIsInvalidString_ShouldReturnNull(string tokenValue)
    {
        var principal = await _tokenProvider.GetPrincipalFromExpiredTokenAsync(tokenValue);

        Assert.Null(principal);
    }

    [Fact]
    public async Task GetPrincipalFromExpiredTokenAsync_WhenAlgorythmIsNotHs256_ShouldReturnNull()
    {
        var longSecret = new string('*', 80);
        var invalidOptions = CopyOptions(secretKey: longSecret);
        var tokenProvider = new JwtTokenProvider(Options.Create(invalidOptions), _clock);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("sub", "123")]),
            Expires = _clock.GetCurrentInstant().Plus(Duration.FromMinutes(15)).ToDateTimeUtc(),
            Issuer = invalidOptions.Issuer,
            Audience = invalidOptions.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(longSecret)), SecurityAlgorithms.HmacSha512),
            TokenType = "at+jwt" // OAuth 2.0 access token type (indicates that the content is a JWT access token)
        };

        var tokenValue = new JsonWebTokenHandler().CreateToken(descriptor);

        var principal = await tokenProvider.GetPrincipalFromExpiredTokenAsync(tokenValue);

        Assert.Null(principal);
    }

    #endregion

    private string GenerateAccessTokenWith(JwtOptions options, IClock? clock = null)
    {
        return new JwtTokenProvider(Options.Create(options), clock ?? _clock)
            .GenerateAccessToken(ApplicationUserInfo, []).AccessToken;
    }

    private JwtOptions CopyOptions(
        string? secretKey = null,
        string? issuer = null,
        string? audience = null,
        int? accessTokenExpirationMinutes = null,
        int? refreshTokenExpirationDays = null)
    {
        return new JwtOptions
        {
            SecretKey = secretKey ?? _jwtOptions.SecretKey,
            Issuer = issuer ?? _jwtOptions.Issuer,
            Audience = audience ?? _jwtOptions.Audience,
            AccessTokenExpirationMinutes = accessTokenExpirationMinutes ?? _jwtOptions.AccessTokenExpirationMinutes,
            RefreshTokenExpirationDays = refreshTokenExpirationDays ?? _jwtOptions.RefreshTokenExpirationDays
        };
    }
}
