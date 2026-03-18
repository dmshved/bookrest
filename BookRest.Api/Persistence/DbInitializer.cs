using BookRest.Api.Constants;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(Roles.Admin))
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

        if (!await roleManager.RoleExistsAsync(Roles.Member))
            await roleManager.CreateAsync(new IdentityRole(Roles.Member));
    }
}
