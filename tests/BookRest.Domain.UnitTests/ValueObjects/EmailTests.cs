using BookRest.Domain.Errors;
using BookRest.Domain.ValueObjects;

namespace BookRest.Domain.UnitTests.ValueObjects;

public sealed class EmailTests
{
    private static Email CreateValidEmail() => Email.Create("email@gmail.com").Value;

    [Fact]
    public static void Create_WhenValidInput_ShouldReturnValidEmail()
    {
        var emailValue = "email@gmail.com";
        var expectedEmailValue = "EMAIL@GMAIL.COM";

        var email = Email.Create(emailValue).Value;

        Assert.Equal(expectedEmailValue, email.Value);
    }

    [Theory]
    [InlineData("EMAIL@GMAIL.COM")]
    [InlineData("Email@Gmail.Com")]
    [InlineData("email@gmail.com")]
    public static void Create_WhenValidInputWithDifferentCases_ShouldReturnValidEmail(string emailValue)
    {
        var expectedEmailValue = "EMAIL@GMAIL.COM";

        var email = Email.Create(emailValue).Value;

        Assert.Equal(expectedEmailValue, email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public static void Create_WhenEmailIsNullOrEmpty_ShouldReturnEmailError(string emailValue)
    {
        var result = Email.Create(emailValue);

        Assert.True(result.IsFailure);
        Assert.Equal(EmailErrors.EmptyEmail(), result.Error);
    }

    [Theory]
    [InlineData("email")] // no @
    [InlineData("@gmail.com")] // no local part
    [InlineData("email@")] // no domain part
    [InlineData("email@gmail")] // no .com
    [InlineData("email@@gmail.com")] // double @
    [InlineData("space email@@gmail.com")] // whitespace
    public static void Create_WhenEmailHasInvalidFormat_ShouldReturnEmailError(string emailValue)
    {
        var result = Email.Create(emailValue);

        Assert.True(result.IsFailure);
        Assert.Equal(EmailErrors.InvalidEmail(), result.Error);
    }

    [Fact]
    public static void Equals_WhenEmailHasDifferentCases_ShouldBeEqual()
    {
        var firstEmail = Email.Create("email@gmail.com").Value;
        var secondEmail = Email.Create("EMAIL@GMAIL.COM").Value;

        Assert.Equal(firstEmail, secondEmail);
    }
}
