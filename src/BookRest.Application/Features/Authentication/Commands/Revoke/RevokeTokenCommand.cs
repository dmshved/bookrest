using BookRest.Application.Common.Interfaces.Messaging;

namespace BookRest.Application.Features.Authentication.Commands.Revoke;

public sealed record RevokeTokenCommand(
    string RefreshToken,
    string? Reason = null) : ICommand;
