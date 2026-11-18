using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record ResendEmailVerificationRequestDto(
    [Required, EmailAddress] string Email);
