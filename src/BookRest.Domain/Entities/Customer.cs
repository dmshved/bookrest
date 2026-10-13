using BookRest.Domain.ValueObjects;

namespace BookRest.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public string UserName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;

    public Guid ApplicationUserId { get; set; }

    private Customer() { }

    public static Customer Create(
        Guid applicationUserId,
        string userName,
        Email email)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = applicationUserId,
            UserName = userName,
            Email = email,
        };
    }

    public void Update(
        string userName,
        Email email)
    {
        UserName = userName;
        Email = email;
    }
}
