using System.Text.RegularExpressions;

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
    /// <returns>A new <see cref="Email"/> instance with the address value normalized to uppercase.</returns>
    /// <exception cref="ArgumentException">Thrown if the input <paramref name="email"/> is null, whitespace or invalid format.</exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }

        return new Email(email.ToUpperInvariant());
    }

    public override string ToString()
    {
        return Value;
    }
}
