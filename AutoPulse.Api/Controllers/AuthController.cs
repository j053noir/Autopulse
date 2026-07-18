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
        /// Authenticates a user and returns access and refresh tokens (and sets secure HTTP-Only cookies).
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

            // Configuración dinámica de cookies para soportar HTTP local y HTTPS en producción
            var isHttps = Request.IsHttps;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(result.ExpiresIn > 0 ? result.ExpiresIn : 60)
            };

            Response.Cookies.Append("autopulse-session", result.AccessToken, cookieOptions);
            Response.Cookies.Append("autopulse-refresh-token", result.RefreshToken, cookieOptions);

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
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand? command, CancellationToken cancellationToken = default)
        {
            // Intentar leer desde las cookies HTTP-only, o caer en el body para compatibilidad heredada
            var accessToken = Request.Cookies["autopulse-session"] ?? command?.AccessToken;
            var refreshToken = Request.Cookies["autopulse-refresh-token"] ?? command?.RefreshToken;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { Message = "Access or Refresh token is missing" });
            }

            var resolvedCommand = new RefreshTokenCommand(accessToken, refreshToken);
            var result = await _mediator.Send(resolvedCommand, cancellationToken);

            if (result is null)
            {
                return BadRequest(new
                {
                    Message = "Invalid or expired token"
                });
            }

            // Actualizar las cookies seguras HTTP-Only con el nuevo par de tokens
            var isHttps = Request.IsHttps;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(result.ExpiresIn > 0 ? result.ExpiresIn : 60)
            };

            Response.Cookies.Append("autopulse-session", result.AccessToken, cookieOptions);
            Response.Cookies.Append("autopulse-refresh-token", result.RefreshToken, cookieOptions);

            return Ok(result);
        }

        /// <summary>
        /// Logs out the user and revokes the active session.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutUserCommand? command, CancellationToken cancellationToken = default)
        {
            // Leer de cookies o caer en el body
            var accessToken = Request.Cookies["autopulse-session"] ?? command?.AccessToken;
            var refreshToken = Request.Cookies["autopulse-refresh-token"] ?? command?.RefreshToken;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                // Si no hay cookies ni datos en el body, borramos las cookies de todas formas y respondemos ok
                Response.Cookies.Delete("autopulse-session");
                Response.Cookies.Delete("autopulse-refresh-token");
                return Ok(new { Message = "Logged out successfully (session was already cleared)" });
            }

            var resolvedCommand = new LogoutUserCommand(accessToken, refreshToken);
            var result = await _mediator.Send(resolvedCommand, cancellationToken);

            // Eliminar cookies del navegador del cliente
            var isHttps = Request.IsHttps;
            var deleteOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };
            Response.Cookies.Delete("autopulse-session", deleteOptions);
            Response.Cookies.Delete("autopulse-refresh-token", deleteOptions);

            if (!result)
            {
                return BadRequest(new
                {
                    Message = "Unable to process logout request in DB"
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
