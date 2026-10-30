namespace BookRest.Domain.ValueObjects;

/// <summary>
/// Represents the source where permissions will be read from.
/// </summary>
public enum PermissionSource
{
    Token,
    Lookup
}
