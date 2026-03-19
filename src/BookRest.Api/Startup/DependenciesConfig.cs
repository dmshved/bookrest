namespace BookRest.Api.Startup;
public static class DependenciesConfig
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddConfiguration(builder.Configuration);
        builder.Services.AddTokenProvider();
        builder.Services.AddIdentityServices();
        builder.Services.AddControllers();
        builder.Services.AddOpenApiServices();
        builder.Services.AddUserAuthServices();
    }
}
