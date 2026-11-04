using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Application.Features.Authentication.Commands.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthenticationResponseDto>;
