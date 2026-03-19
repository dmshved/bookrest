using BookRest.Api.Infrastructure;

namespace BookRest.Api.Startup;

public static class TokenProviderConfig
{
    public static void AddTokenProvider(this IServiceCollection services)
    {
        services.AddSingleton<JwtTokenProvider>();
    }
}
