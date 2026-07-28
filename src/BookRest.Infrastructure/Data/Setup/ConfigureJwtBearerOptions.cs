using System.Text;
using BookRest.Infrastructure.Data.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookRest.Infrastructure.Data.ConfigureOptions;

public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptions<JwtOptions> _options;

    public ConfigureJwtBearerOptions(IOptions<JwtOptions> options)
    {
        _options = options;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        JwtOptions jwtOptions = _options.Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
        };
    }
}