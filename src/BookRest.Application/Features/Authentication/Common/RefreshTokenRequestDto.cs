using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record RefreshTokenRequestDto(
    [Required] string RefreshToken);
