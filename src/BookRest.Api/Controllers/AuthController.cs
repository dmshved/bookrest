using BookRest.Api.Abstractions;
using BookRest.Application.Features.Authentication.Commands.Login;
using BookRest.Application.Features.Authentication.Commands.RegisterCustomer;
using BookRest.Application.Features.Authentication.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Controllers;

[Route("api/v1/[controller]")]
public class AuthController : ApiController
{
    public AuthController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Authenticate user and return JWT tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }

    /// <summary>
    /// Register new customer and return JWT tokens.
    /// </summary>
    [HttpPost("customers/register")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequestDto request)
    {
        var command = new RegisterCustomerCommand(request.UserName, request.Email, request.Password, request.ConfirmPassword);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }
}
