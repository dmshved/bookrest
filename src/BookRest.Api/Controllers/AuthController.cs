using Asp.Versioning;
using BookRest.Api.Abstractions;
using BookRest.Api.Constants;
using BookRest.Application.Features.Authentication.Commands.Login;
using BookRest.Application.Features.Authentication.Commands.Refresh;
using BookRest.Application.Features.Authentication.Commands.RegisterCustomer;
using BookRest.Application.Features.Authentication.Commands.RegisterOwner;
using BookRest.Application.Features.Authentication.Commands.ResendEmailVerification;
using BookRest.Application.Features.Authentication.Commands.Revoke;
using BookRest.Application.Features.Authentication.Commands.VerifyEmail;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
public class AuthController : ApiController
{
    public AuthController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Authenticate user and return JWT tokens.
    /// </summary>
    [MapToApiVersion(1)]
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
    [MapToApiVersion(1)]
    [HttpPost("customers/register")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequestDto request)
    {
        var command = new RegisterCustomerCommand(request.UserName, request.Email, request.Password, request.ConfirmPassword);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }

    /// <summary>
    /// Register new owner and return JWT tokens.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpPost("owners/register")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerRequestDto request)
    {
        var command = new RegisterOwnerCommand(request.UserName, request.Email, request.Password, request.ConfirmPassword);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }

    /// <summary>
    /// Exchange a refresh token for a new pair of refresh token and access token.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }

    /// <summary>
    /// Revoke a refresh tokenn.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpPost("revoke")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequestDto request)
    {
        var command = new RevokeTokenCommand(request.RefreshToken, request.Reason);
        var result = await Sender.Send(command);

        return ToHttpResult(result, Ok);
    }

    /// <summary>
    /// Confirm an email address using the link sent by email.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpGet("verify-email", Name = AuthRouteNames.VerifyEmail)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] Guid applicationUserId, [FromQuery] string confirmationToken)
    {
        var command = new VerifyEmailCommand(applicationUserId, confirmationToken);
        var result = await Sender.Send(command);

        return ToHttpResult(result, Ok);
    }

    /// <summary>
    /// Send a new verification email.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpPost("verify-email/resend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequestDto request)
    {
        var command = new ResendEmailVerificationCommand(request.Email);
        var result = await Sender.Send(command);

        return ToHttpResult(result, Ok);
    }
}
