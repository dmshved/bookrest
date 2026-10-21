using BookRest.Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static Result<T> ToApplicationResult<T>(this IdentityResult result, T value)
    {
        return result.Succeeded
            ? Result<T>.Success(value)
            : Result<T>.Failure(
                new Error("Identity.OperationFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description)),
                    ErrorType.Validation));
    }

    public static Result ToApplicationResult(this IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(
                new Error("Identity.OperationFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description)),
                    ErrorType.Validation));
    }
}
