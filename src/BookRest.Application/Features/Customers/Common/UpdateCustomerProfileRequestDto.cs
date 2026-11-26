using System.ComponentModel.DataAnnotations;

namespace BookRest.Application.Features.Customers.Common;

public record UpdateCustomerProfileRequestDto(
    [Required] string UserName);
