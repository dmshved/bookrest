namespace BookRest.Domain.Errors;

/// <summary>
/// Represents domain customer errors.
/// </summary>
public static class CustomerErrors
{
    public static Error EmptyEmail() =>
        new("Customer.EmptyEmail",
            "Email cannot be empty.",
            ErrorType.Validation);

    public static Error InvalidEmail() =>
        new("Customer.InvalidEmail",
            "Invalid email format.",
            ErrorType.Validation);

    public static Error NotFoundById(Guid id) =>
        new("Customer.NotFoundById",
            $"Customer with specified id '{id}' was not found.",
            ErrorType.NotFound);

    public static Error NotFoundByEmail(string email) =>
        new("Customer.NotFoundByEmail",
            $"Customer with specified email '{email}' was not found.",
            ErrorType.NotFound);
}


