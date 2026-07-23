using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Identity;

namespace ProductManagement.API.Controllers.v1
{
    /// <summary>
    /// Controller managing User Authentication (Version 1).
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [AllowAnonymous] // Authentication endpoints are accessible anonymously
    public class AuthController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registers a new User.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Authenticates a User and returns access and refresh tokens.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Exchanges an active Refresh Token for a new Access Token and Refresh Token.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Revokes the specified Refresh Token, logging the User out.
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            var command = new LogoutCommand(request.RefreshToken);
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }
    }
}
