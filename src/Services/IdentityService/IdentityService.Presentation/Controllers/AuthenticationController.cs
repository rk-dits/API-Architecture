using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using IdentityService.Application.Authentication.Commands;
using IdentityService.Application.Authentication.Queries;
using IdentityService.Contracts.Authentication;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace IdentityService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticate user and return access token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with tokens and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid credentials or validation error</response>
    /// <response code="401">Authentication failed</response>
    /// <response code="423">Account locked due to too many failed attempts</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Locked)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var command = new LoginCommand(
                request.Username,
                request.Password,
                request.MfaCode,
                request.RememberMe,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers.UserAgent.ToString()
            );

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return result.Message switch
                {
                    var msg when msg?.Contains("locked", StringComparison.OrdinalIgnoreCase) == true => StatusCode((int)HttpStatusCode.Locked, result),
                    var msg when msg?.Contains("invalid", StringComparison.OrdinalIgnoreCase) == true => Unauthorized(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred during authentication" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access and refresh tokens</returns>
    /// <response code="200">Token refresh successful</response>
    /// <response code="400">Invalid or expired refresh token</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("refresh")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(RefreshTokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Invalid refresh token" });
        }

        try
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Add security headers
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            Response.Headers.Add("X-Frame-Options", "DENY");

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logout user and invalidate tokens
    /// </summary>
    /// <param name="request">Logout request with optional refresh token</param>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessionId = HttpContext.Items["SessionId"]?.ToString();

            var command = new LogoutCommand(request?.RefreshToken, sessionId);
            var result = await _mediator.Send(command);

            // Clear any authentication cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith("auth", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            return Ok(new
            {
                success = result,
                message = "Logout successful",
                timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetUserQuery(userId);
            var user = await _mediator.Send(query);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Password change confirmation</returns>
    /// <response code="200">Password changed successfully</response>
    /// <response code="400">Invalid request or password policy violation</response>
    /// <response code="401">Unauthorized or incorrect current password</response>
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "Current password and new password are required" });
        }

        if (request.CurrentPassword == request.NewPassword)
        {
            return BadRequest(new { message = "New password must be different from current password" });
        }

        try
        {
            var userId = GetCurrentUserId();
            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to change password. Please check your current password." });
            }

            return Ok(new
            {
                success = true,
                message = "Password changed successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred while changing password" });
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command);

        return Ok(new { success = result });
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword);
        var result = await _mediator.Send(command);

        return Ok(new { success = result });
    }

    /// <summary>
    /// Setup Multi-Factor Authentication
    /// </summary>
    [HttpPost("setup-mfa")]
    [Authorize]
    public async Task<ActionResult<SetupMfaResponse>> SetupMfa([FromBody] SetupMfaRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new SetupMfaCommand(userId, request.Method);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Disable Multi-Factor Authentication
    /// </summary>
    [HttpPost("disable-mfa")]
    [Authorize]
    public async Task<ActionResult> DisableMfa([FromBody] DisableMfaRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new DisableMfaCommand(userId, request.Password, request.MfaCode);
        var result = await _mediator.Send(command);

        return Ok(new { success = result });
    }

    /// <summary>
    /// Verify MFA code
    /// </summary>
    [HttpPost("verify-mfa")]
    public async Task<ActionResult> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        var command = new VerifyMfaCommand(Guid.Parse(request.UserId), request.Code);
        var result = await _mediator.Send(command);

        return Ok(new { success = result });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}