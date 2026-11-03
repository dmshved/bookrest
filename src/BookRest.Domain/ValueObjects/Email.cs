using System.Text.RegularExpressions;
using BookRest.Domain.Errors;

namespace BookRest.Domain.ValueObjects;

/// <summary>
/// Represents an immutable record of Email.
/// </summary>
public sealed record Email
{
    public string Value { get; } = string.Empty;

    private Email() { }

    private Email(string value) => Value = value;

    /// <summary>
    /// Creates and validates a new <see cref="Email"/> instance.
    /// </summary>
    /// <param name="email">Email address.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="Email"/> normalized to uppercase,
    /// or a failure if the input is null, white space or has an invalid format.
    /// </returns>
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Failure(EmailErrors.EmptyEmail());
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
        {
            return Result<Email>.Failure(EmailErrors.InvalidEmail());
        }

        var validEmail = new Email(email.ToUpperInvariant());

        return Result<Email>.Success(validEmail);
    }

    public override string ToString()
    {
        return Value;
    }
}
