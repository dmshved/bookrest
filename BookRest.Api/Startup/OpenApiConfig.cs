using Scalar.AspNetCore;

namespace BookRest.Api.Startup;
public static class OpenApiConfig
{
    public static void AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi();
    }
    public static void UseOpenApi(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "Bookrest API";
            options.Theme = ScalarTheme.Kepler;
            options.HideClientButton = true;
        });
    }
}
