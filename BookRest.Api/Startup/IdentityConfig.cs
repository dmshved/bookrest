using BookRest.Api.Data;
using BookRest.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Api.Startup;

public static class IdentityConfig
{
    public static void AddIdentityServices(this IServiceCollection builder)
    {
        builder.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
    }
}
