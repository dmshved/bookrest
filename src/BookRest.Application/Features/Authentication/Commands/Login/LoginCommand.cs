using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Application.Features.Authentication.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : ICommand<AuthenticationResponseDto>;
