namespace BookRest.Application.Features.Customers.Commands.UpdateCustomerProfile;

public sealed record UpdateCustomerProfileCommand(
    string UserName) : ICommand;
