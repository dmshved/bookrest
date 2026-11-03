namespace BookRest.Domain.Errors;

/// <summary>
/// Represents domain email errors.
/// </summary>
public static class EmailErrors
{
    public static Error EmptyEmail() =>
        new("Email.EmptyEmail",
            "Email cannot be empty.",
            ErrorType.Validation);

    public static Error InvalidEmail() =>
        new("Email.InvalidEmail",
            "Invalid email format.",
            ErrorType.Validation);
}
