using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Features.Authentication.Common;

namespace BookRest.Application.Features.Authentication.Commands.RegisterOwner;

public sealed record RegisterOwnerCommand(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand<AuthenticationResponseDto>;
