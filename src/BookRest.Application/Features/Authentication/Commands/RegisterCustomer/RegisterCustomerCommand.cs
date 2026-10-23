using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Application.Features.Authentication.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand<AuthenticationResponseDto>;
