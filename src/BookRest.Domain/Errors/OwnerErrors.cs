namespace BookRest.Domain.Errors;

/// <summary>
/// Represents domain owner errors.
/// </summary>
public static class OwnerErrors
{
    public static Error AlreadyExists(string email) =>
        new("Owner.AlreadyExists",
            $"Owner with specified '{email}' already exists.",
            ErrorType.Conflict);

    public static Error NotFoundById(Guid id) =>
        new("Owner.NotFoundById",
            $"Owner with specified id '{id}' was not found.",
            ErrorType.NotFound);

    public static Error NotFoundByEmail(string email) =>
        new("Owner.NotFoundByEmail",
            $"Owner with specified email '{email}' was not found.",
            ErrorType.NotFound);
}
