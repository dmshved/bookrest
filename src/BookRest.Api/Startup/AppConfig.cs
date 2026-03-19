using BookRest.Api.Configuration;

namespace BookRest.Api.Startup;

public static class AppConfig
{
    public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguration>(configuration.GetSection("Jwt"));
    }
}
 