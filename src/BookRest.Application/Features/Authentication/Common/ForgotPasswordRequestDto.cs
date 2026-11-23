using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record ForgotPasswordRequestDto(
    [Required, EmailAddress] string Email);
