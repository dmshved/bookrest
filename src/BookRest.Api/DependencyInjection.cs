using BookRest.Api.Services;
using BookRest.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

// Placing the extension method in this namespace makes it
// available without requiring an additional using.
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        // Configure default API behaviour so I have the full control over exception handling.
        // The default behaviour sets 400 error code in case validation of the incoming data failed.
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddControllers();

        builder.Services.AddRouting(options =>
            options.LowercaseUrls = true);

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        builder.Services.AddVersioning();

        builder.Services.AddCors();

        builder.Services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            // Enable only when fully ready https://hstspreload.org/
            options.Preload = false;
        });
    }
}
