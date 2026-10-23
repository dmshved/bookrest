using BookRest.Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static Result<T> ToApplicationResult<T>(this IdentityResult result, T value)
    {
        return result.Succeeded
            ? ValidationResult<T>.Success(value)
            : ValidationResult<T>.WithErrors(result.Errors
                .Select(error => new Error(
                    code: error.Code,
                    description: error.Description,
                    errorType: ErrorType.Validation))
                .ToArray());
    }

    public static Result ToApplicationResult(this IdentityResult result)
    {
        return result.Succeeded
            ? ValidationResult.Success()
            : ValidationResult.WithErrors(result.Errors
                .Select(error => new Error(
                    code: error.Code,
                    description: error.Description,
                    errorType: ErrorType.Validation))
                .ToArray());
    }
}
