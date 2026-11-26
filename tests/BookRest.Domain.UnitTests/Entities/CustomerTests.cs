using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;

namespace BookRest.Domain.UnitTests.Entities;

public sealed class CustomerTests
{
    private static Customer CreateValidCustomer() => Customer.Create(
        applicationUserId: Guid.NewGuid(),
        userName: "customer",
        email: Email.Create("customer@gmail.com").Value);

    [Fact]
    public static void Create_WhenValidInput_ShouldCreateValidCustomer()
    {
        var applicationUserId = Guid.NewGuid();
        var userName = "customer";
        var email = Email.Create("customer@gmail.com").Value;

        var customer = Customer.Create(applicationUserId, userName, email);

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(applicationUserId, customer.ApplicationUserId);
        Assert.Equal(userName, customer.UserName);
        Assert.Equal(email, customer.Email);
    }

    [Fact]
    public static void UpdateProfile_WhenValidInput_ShouldUpdateCustomerProfile()
    {
        var customer = CreateValidCustomer();
        var originalId = customer.Id;
        var originalApplicationUserId = customer.ApplicationUserId;
        var newUserName = "updated";

        customer.UpdateProfile(newUserName);

        Assert.Equal(originalId, customer.Id);
        Assert.Equal(originalApplicationUserId, customer.ApplicationUserId);
        Assert.Equal(newUserName, customer.UserName);
    }
}
