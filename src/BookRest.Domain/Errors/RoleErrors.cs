namespace BookRest.Domain.Errors;

/// <summary>
/// Represents domain role errors.
/// </summary>
public static class RoleErrors
{
    public static Error NotFound(string roleName) =>
        new("Role.NotFound",
            $"Role with name '{roleName}' was not found.",
            ErrorType.NotFound);
}
