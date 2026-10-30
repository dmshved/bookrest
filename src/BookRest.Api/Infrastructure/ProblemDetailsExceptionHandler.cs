// Learn more: https://www.milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-from-middleware-to-modern-handlers#chaining-exception-handlers

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Infrastructure;

/// <summary>
/// Converts well-known application exceptions into RFC 9110-compliant <see cref="ProblemDetails"/> responses,
/// mapping  <see cref="NotFoundException"/> - 404, <see cref="UnauthorizedAccessException"/> - 401,
/// and <see cref="ForbiddenAccessException"/> - 403. Unrecognised exceptions are not handled and fall through to the default middleware.
/// </summary>
public class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ProblemDetailsExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid argument provided",
                Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.1"
            }),
            ArgumentException => (StatusCodes.Status400BadRequest, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Invalid argument provided",
                Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.1"
            }),
            _ => (StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.6.1"
            })
        };

        if (problemDetails is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
