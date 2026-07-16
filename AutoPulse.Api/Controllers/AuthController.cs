using AutoPulse.Application.Application.Authentication.Commands.LoginUser;
using AutoPulse.Application.Application.Authentication.Commands.RegisterUser;
using AutoPulse.Application.Application.Authentication.Commands.RefreshToken;
using AutoPulse.Application.Application.Authentication.Commands.LogoutUser;
using AutoPulse.Application.Application.Authentication.Queries.GetCurrentUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Authenticates a user and returns access and refresh tokens.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    Message = $"Credentials not found"
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(new Dictionary<string, Guid> { { "id", result } });
        }

        /// <summary>
        /// Rotates the refresh token and returns a new pair of access and refresh tokens.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (result is null)
            {
                return BadRequest(new
                {
                    Message = "Invalid or expired token"
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Logs out the user and revokes the active session.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutUserCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return BadRequest(new
                {
                    Message = "Unable to process logout request"
                });
            }

            return Ok(new { Message = "Logged out successfully" });
        }

        /// <summary>
        /// Retrieves the current authenticated user's profile and active permissions.
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var userId = GetUserClaimId("UserId");
            var familyId = User.Claims.FirstOrDefault(c => c.Type == "family_id")?.Value ?? string.Empty;

            var query = new GetCurrentUserProfileQuery(userId, familyId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
