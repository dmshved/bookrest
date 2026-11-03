using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;

namespace BookRest.Domain.UnitTests.Entities;

public sealed class CustomerTests
{
    private static Customer CreateValidCustomer() => Customer.Create(
        applicationUserId: Guid.NewGuid(),
        userName: "customer",
        email: Email.Create("customer@gmail.com"));

    [Fact]
    public static void Create_WhenValidInput_ShouldCreateValidCustomer()
    {
        var applicationUserId = Guid.NewGuid();
        var userName = "customer";
        var email = Email.Create("customer@gmail.com");

        var customer = Customer.Create(applicationUserId, userName, email);

        Assert.NotEqual(customer.Id, Guid.Empty);
        Assert.Equal(customer.ApplicationUserId, applicationUserId);
        Assert.Equal(customer.UserName, userName);
        Assert.Equal(customer.Email, email);
    }

    [Fact]
    public static void Update_WhenValidInput_ShouldUpdateCustomer()
    {
        var customer = CreateValidCustomer();
        var originalId = customer.Id;
        var originalApplicationUserId = customer.ApplicationUserId;
        var newEmail = Email.Create("updated@gmail.com");
        var newUserName = "updated";

        customer.Update(newUserName, newEmail);

        Assert.Equal(customer.Id, originalId);
        Assert.Equal(customer.ApplicationUserId, originalApplicationUserId);
        Assert.Equal(customer.UserName, newUserName);
        Assert.Equal(customer.Email, newEmail);
    }
}
