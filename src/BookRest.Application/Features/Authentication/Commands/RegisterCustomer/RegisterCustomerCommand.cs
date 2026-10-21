using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword) : IRequest<Result<AuthenticationResponseDto>>;
