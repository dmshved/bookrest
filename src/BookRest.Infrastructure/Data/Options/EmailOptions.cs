namespace BookRest.Infrastructure.Data.Options;

public class EmailOptions
{
    public const string SectionName = "EmailOptions";

    public string FromAddress { get; set; } = string.Empty;
    public string? FromDisplayName { get; set; }
}
