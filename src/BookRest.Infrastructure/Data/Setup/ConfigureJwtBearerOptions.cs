using System.Text;
using BookRest.Infrastructure.Data.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookRest.Infrastructure.Data.Setup;

public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;

    public ConfigureJwtBearerOptions(IOptions<JwtOptions> options)
    {
        _jwtOptions = options.Value;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        // Keep claim names exactly as they appear in the token
        // If you leave legacy mapping on, claims will be renamed
        // to the long ClaimTypes.* URI and for example FindFirstValue("application_user_id") quietly returns null
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
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
            RoleClaimType = "role"
        };
    }
}
