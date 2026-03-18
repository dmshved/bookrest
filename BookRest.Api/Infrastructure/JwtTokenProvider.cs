// REMINDER: I'm using the Options Pattern (https://dev.to/stevsharp/using-options-pattern-in-aspnet-core-1cc8)
// to be able to use the jwt configuration with strong typed data

using BookRest.Api.Configuration;
using BookRest.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BookRest.Api.Infrastructure;

internal sealed class JwtTokenProvider(IOptions<JwtConfiguration> jwtConfiguration)
{
    // Set the IOptions<JwtConfiguration> Value to the readonly field
    private readonly JwtConfiguration _jwtConfiguration = jwtConfiguration.Value;

    public string GenerateAccessToken(AppUser user, IList<string> roles)
    {
        // Secret key, issuer, audience, expiration 
        //
        // Get the secret key from bytes of access key from user secrets
        var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey));

        // This object contains signinKey + algorithm, without this object we can't sign our JWTs
        var credentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);

        // Define JWT claims (the part that describes a client in the payload)
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        // Define the tokenDescriptor (a template describing how JWTs will be created)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfiguration.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtConfiguration.Issuer,
            Audience = _jwtConfiguration.Audience,
        };

        var tokenHandler = new JsonWebTokenHandler();

        // Generate the AccessToken
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return accessToken;
    }
    public string GenerateRefreshToken()
    {
        // The usage of RandomNumberGenerator.GetBytes() increases the security
        // because it returns an array with criptographically strong random values.
        // This way it is unreal to guess the refresh token creation process even if someone stole one
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
