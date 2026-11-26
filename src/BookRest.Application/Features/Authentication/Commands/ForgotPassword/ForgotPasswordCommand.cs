namespace BookRest.Application.Features.Authentication.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand;
