namespace BookRest.Api.Startup;

public static class DevelopmentConfig
{
    public static void UseDevelopmentServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
        }
    }
}
