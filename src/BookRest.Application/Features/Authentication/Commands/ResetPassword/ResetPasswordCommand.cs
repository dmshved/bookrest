using BookRest.Application.Common.Interfaces.Messaging;

namespace BookRest.Application.Features.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid ApplicationUserId,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword) : ICommand;
