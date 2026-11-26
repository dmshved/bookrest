namespace BookRest.Application.Features.Authentication.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(
    Guid ApplicationUserId,
    string EmailVerificationToken) : ICommand;
