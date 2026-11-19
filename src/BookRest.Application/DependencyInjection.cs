using System.Reflection;
using BookRest.Application;
using BookRest.Application.Common.Behaviours;
using BookRest.Application.Common.Interfaces.Emailing;
using Microsoft.Extensions.Hosting;

// Placing the extension method in this namespace makes it
// available without requiring an additional using.
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(config =>
            config.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            config.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            config.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            config.AddOpenBehavior(typeof(PerfomanceBehaviour<,>));
        });

        builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
    }
}
