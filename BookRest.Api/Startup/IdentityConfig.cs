using BookRest.Api.Data;
using BookRest.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Api.Startup;

public static class IdentityConfig
{
    public static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
    }
}
