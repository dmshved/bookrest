using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthenticationResponseDto>>;
