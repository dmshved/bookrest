using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record ResetPasswordRequestDto(
    [Required] Guid ApplicationUserId,
    [Required] string ResetToken,
    [Required] string NewPassword,
    [Required] string ConfirmPassword);
