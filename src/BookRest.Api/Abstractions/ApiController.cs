using BookRest.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Abstractions;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected readonly ISender Sender;

    protected ApiController(ISender sender)
    {
        Sender = sender;
    }

    /// <summary>
    /// Maps a generic Result into IActionResult with proper status codes.
    /// </summary>
    public IActionResult ToHttpResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return ToProblem(result);
    }

    /// <summary>
    /// Maps a Result into IActionResult with proper status codes.
    /// </summary>
    public IActionResult ToHttpResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ToProblem(result);
    }

    /// <summary>
    /// Maps a failed Result into RFC 9110-compliant <see cref="ProblemDetails"/> responses.
    /// </summary>
    private IActionResult ToProblem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot return problem details on successful result.");
        }

        var error = result.Error!.Value;

        var (statusCode, problemDetails) = result switch
        {
            // // Map validation errors
            IValidationResult validationResult => (StatusCodes.Status400BadRequest, (ProblemDetails)new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(e => e.Description).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation error occured."
            }),
            // Map other standard errors
            _ => error.ErrorType switch
            {
                ErrorType.NotFound => (StatusCodes.Status404NotFound, new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = "Not found",
                    Detail = error.Description
                }),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.10",
                    Title = "Conflict",
                    Detail = error.Description,
                }),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.2",
                    Title = "Unauthorized",
                    Detail = error.Description
                }),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://www.rfc-editor.org/info/rfc9110/#section-15.5.4",
                    Title = "Forbidden",
                    Detail = error.Description
                }),
                _ => (StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Internal Server Error",
                        Detail = error.Description
                    })
            }
        };

        // Add code to the problem details response
        problemDetails.Extensions["code"] = error.Code;

        return StatusCode(statusCode, problemDetails);
    }
}
