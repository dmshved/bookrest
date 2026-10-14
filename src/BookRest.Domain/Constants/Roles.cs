namespace BookRest.Domain.Constants;

public static class Roles
{
    public const string Customer = nameof(Customer);
    public const string Owner = nameof(Owner);
    public const string Administrator = nameof(Administrator);

    public static IReadOnlyList<string> All()
    {
        return
        [
            Customer,
            Owner,
            Administrator
        ];
    }
}
