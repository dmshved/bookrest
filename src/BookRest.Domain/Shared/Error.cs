namespace BookRest.Domain.Shared;

/// <summary>
/// Represents an error with a machine-readable code and human-readable message.
/// </summary>
public readonly record struct Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    public Error(string code, string message, ErrorType errorType = ErrorType.Failure)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
    }

    public override string ToString() => $"[{Code}]: {Message}";
}
