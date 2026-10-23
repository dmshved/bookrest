namespace BookRest.Domain.Shared;

/// <summary>
/// Represents an error with a machine-readable code and human-readable message.
/// </summary>
public readonly record struct Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType ErrorType { get; }

    public Error(string code, string description, ErrorType errorType = ErrorType.Failure)
    {
        Code = code;
        Description = description;
        ErrorType = errorType;
    }

    public static readonly Error None = new(string.Empty, string.Empty);

    public override string ToString() => $"[{Code}]: {Description}";
}
