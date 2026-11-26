using Asp.Versioning;
using BookRest.Api.Abstractions;
using BookRest.Application.Features.Customers.Commands.UpdateCustomerProfile;
using BookRest.Application.Features.Customers.Common;
using BookRest.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
[Authorize]
public class CustomersController : ApiController
{
    public CustomersController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Updates the user profile.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpPut("me")]
    [Authorize(Policy = Permissions.Customers.Edit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequestDto request)
    {
        var command = new UpdateCustomerProfileCommand(request.UserName);
        var result = await Sender.Send(command);

        return ToHttpResult(result, Ok);
    }
}
