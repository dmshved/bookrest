using BookRest.Api.Features;
using BookRest.Api.Services;

namespace BookRest.Api.Startup;

public static class AuthServicesConfig
{
    public static void AddUserAuthServices(this IServiceCollection services)
    {
        services.AddScoped<LoginUser>();
        services.AddScoped<RegisterUser>();
        services.AddScoped<LoginUserWithRefreshToken>();
        services.AddScoped<RevokeRefreshTokens>();
    }
}
