using BookRest.Domain.ValueObjects;

namespace BookRest.Infrastructure.Data.Options;

public class PermissionOptions
{
    public const string SectionName = "PermissionOptions";

    public PermissionSource PermissionSource { get; set; }
}
