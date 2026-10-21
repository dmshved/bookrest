using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Authentication.Common;

public record RegisterCustomerRequestDto(
    [Required] string UserName,
    [Required] string Password,
    [Required] string ConfirmPassword,
    [Required, EmailAddress] string Email);
