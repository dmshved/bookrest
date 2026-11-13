using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;

namespace BookRest.Domain.UnitTests.Entities;

public sealed class OwnerTests
{
    private static Owner createValidOwner() => Owner.Create(
        applicationUserId: Guid.NewGuid(),
        userName: "owner",
        email: Email.Create("owner@gmail.com").Value);

    [Fact]
    public static void Create_WhenValidInput_ShouldCreateValidCustomer()
    {
        var applicationUserId = Guid.NewGuid();
        var userName = "owner";
        var email = Email.Create("owner@gmail.com").Value;

        var owner = Customer.Create(applicationUserId, userName, email);

        Assert.NotEqual(Guid.Empty, owner.Id);
        Assert.Equal(applicationUserId, owner.ApplicationUserId);
        Assert.Equal(userName, owner.UserName);
        Assert.Equal(email, owner.Email);
    }

    [Fact]
    public static void Update_WhenValidInput_ShouldUpdateCustomer()
    {
        var owner = createValidOwner();
        var originalId = owner.Id;
        var originalApplicationUserId = owner.ApplicationUserId;
        var newEmail = Email.Create("updated@gmail.com").Value;
        var newUserName = "updated";

        owner.Update(newUserName, newEmail);

        Assert.Equal(originalId, owner.Id);
        Assert.Equal(originalApplicationUserId, owner.ApplicationUserId);
        Assert.Equal(newUserName, owner.UserName);
        Assert.Equal(newEmail, owner.Email);
    }
}
