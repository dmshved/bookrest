using BookRest.Domain.ValueObjects;

namespace BookRest.Domain.Entities;

public class Owner : BaseAuditableEntity
{
    public string UserName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;

    public Guid ApplicationUserId { get; set; }

    private Owner() { }

    public static Owner Create(
        Guid applicationUserId,
        string userName,
        Email email)
    {
        return new Owner
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
