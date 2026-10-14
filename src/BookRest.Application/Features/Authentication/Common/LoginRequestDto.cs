using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record LoginRequestDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
