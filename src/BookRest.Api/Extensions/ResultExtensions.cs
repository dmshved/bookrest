using BookRest.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a generic Result into IActionResult with proper status codes.
    /// </summary>
    public static IActionResult ToHttpResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        var error = result.Error!.Value;

        return ToProblem(error, controller);
    }

    /// <summary>
    /// Maps a Result into IActionResult with proper status codes.
    /// </summary>
    public static IActionResult ToHttpResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        var error = result.Error!.Value;

        return ToProblem(error, controller);
    }

    /// <summary>
    /// Maps Error into RFC 9110-compliant <see cref="ProblemDetails"/> responses.
    /// </summary>
    private static IActionResult ToProblem(Error error, ControllerBase controller)
    {
        var (statusCode, problemDetails) = error.ErrorType switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "Not found",
                Detail = error.Message,
            }),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Bad request",
                Detail = error.Message
            }),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.10",
                Title = "Conflict",
                Detail = error.Message
            }),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.2",
                Title = "Unauthorized",
                Detail = error.Message
            }),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.4",
                Title = "Forbidden",
                Detail = error.Message
            }),
            _ => (StatusCodes.Status500InternalServerError,
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = error.Message
            })
        };

        // Add error code into problem details response
        problemDetails.Extensions["code"] = error.Code;

        return controller.StatusCode(statusCode, problemDetails);
    }
}
