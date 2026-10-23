namespace BookRest.Domain.Shared;

public interface IValidationResult
{
    public static readonly Error ValidationError = new(
        "ValidationError",
        "One or more validation error occured.",
        ErrorType.Validation);

    Error[] Errors { get; }
}
