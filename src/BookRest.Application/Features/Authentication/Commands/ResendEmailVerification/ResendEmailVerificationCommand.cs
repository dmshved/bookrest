using BookRest.Application.Common.Interfaces.Messaging;

namespace BookRest.Application.Features.Authentication.Commands.ResendEmailVerification;

public sealed record ResendEmailVerificationCommand(string Email) : ICommand;
