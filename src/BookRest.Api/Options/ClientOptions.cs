namespace BookRest.Api.Options;

public class ClientOptions
{
    public const string SectionName = "ClientOptions";

    /// <summary>
    /// Gets or sets the frontend page url that shows the reset password form.
    /// </summary>
    public string ResetPasswordUrl { get; set; } = string.Empty;
}
