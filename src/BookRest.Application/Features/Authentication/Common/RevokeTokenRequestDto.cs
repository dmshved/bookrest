using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record RevokeTokenRequestDto(
    [Required] string RefreshToken,
    string Reason);
