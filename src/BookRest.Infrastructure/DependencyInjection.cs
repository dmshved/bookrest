using System.Reflection;
using BookRest.Infrastructure.Data;
using BookRest.Infrastructure.Data.Interceptors;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Infrastructure.Data.Options;
using BookRest.Infrastructure.Data.Repositories;
using BookRest.Infrastructure.Data.Setup;
using BookRest.Infrastructure.Identity.Models;
using BookRest.Infrastructure.Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using NodaTime.Extensions;

// Placing the extension method in this namespace makes it
// available without requiring an additional using.
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);

        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());

            options.UseNpgsql(connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNodaTime();
                    npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
                });

            // The early development configuration, remove at the Late Development stage
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName);

        builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        builder.Services.AddAuthorizationBuilder();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                // Set to true in production
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddSingleton<IClock>(serviceProvider =>
        {
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            return timeProvider.ToClock();
        });

        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddSingleton<ITokenProvider, JwtTokenProvider>();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }
}
