using System.Reflection;
using BookRest.Application.Common.Behaviours;
using Microsoft.Extensions.Hosting;

// Placing the extension method in this namespace makes it 
// available without requiring an additional using.
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // TODO: Register AutoMapper profiles
        // TODO: Register FluentValidation validators

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>)); 
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerfomanceBehaviour<,>));
        });
    }
}