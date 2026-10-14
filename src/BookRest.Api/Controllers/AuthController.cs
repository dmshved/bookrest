using BookRest.Application.Features.Authentication.Commands.Login;
using BookRest.Application.Features.Authentication.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
