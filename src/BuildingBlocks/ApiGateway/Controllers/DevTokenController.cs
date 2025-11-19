
using BuildingBlocks.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Controllers;

/// <summary>
/// Development controller for generating JWT tokens for testing
/// Available only in development environment
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DevTokenController : ControllerBase
{
    private readonly JwtTokenService _tokenService;
    private readonly IWebHostEnvironment _environment;

    public DevTokenController(IOptions<JwtAuthenticationOptions> options, IWebHostEnvironment environment)
    {
        _tokenService = new JwtTokenService(options);
        _environment = environment;
    }

    /// <summary>
    /// Generates a development JWT token with predefined scopes
    /// </summary>
    /// <returns>JWT token for development use</returns>
    /// <summary>
    /// Generates a development JWT token with predefined scopes.
    /// </summary>
    /// <remarks>
    /// <b>DEPRECATED:</b> This endpoint is for development use only and may be removed in future releases. See sunset policy in response headers.
    /// </remarks>
    /// <param name="request">Optional request to customize the generated token.</param>
    /// <returns>JWT token for development use.</returns>
    [Obsolete("This endpoint is for development use only and may be removed in future releases.")]
    [HttpPost("generate")]
    [ProducesResponseType(typeof(DevTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // TODO: Add [SwaggerOperation] attribute if/when compatible with Swashbuckle.AspNetCore.Filters v10+
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DevTokenResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundExample))]
    // TODO: Add [SwaggerResponseHeader] attribute if/when compatible with Swashbuckle.AspNetCore.Filters v10+
    public IActionResult GenerateDevToken([FromBody] DevTokenRequest? request = null)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var userId = request?.UserId ?? "dev-user";
        var email = request?.Email ?? "dev@acme.local";
        var roles = request?.Roles ?? new[] { "User", "Developer" };
        var scopes = request?.Scopes ?? new[] { "integrations.access", "workflows.access", "admin.access" };

        var token = _tokenService.GenerateToken(userId, email, roles, scopes);

        Response.Headers.Append("Sunset", "2026-01-01");
        return Ok(new DevTokenResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresInMinutes = 60,
            UserId = userId,
            Email = email,
            Roles = roles,
            Scopes = scopes,
            Instructions = new[]
            {
                "Use this token in the Authorization header as: Bearer {token}",
                "This token is valid for 60 minutes",
                "This endpoint is only available in development environment"
            }
        });
    }

    /// <summary>
    /// Validates a JWT token and returns its claims.
    /// </summary>
    /// <remarks>
    /// <b>DEPRECATED:</b> This endpoint is for development use only and may be removed in future releases. See sunset policy in response headers.
    /// </remarks>
    /// <param name="request">Request containing the JWT token to validate.</param>
    /// <returns>Validation result and claims if valid.</returns>
    [Obsolete("This endpoint is for development use only and may be removed in future releases.")]
    [HttpPost("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // TODO: Add [SwaggerOperation] attribute if/when compatible with Swashbuckle.AspNetCore.Filters v10+
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ValidateTokenResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InvalidTokenExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundExample))]
    // TODO: Add [SwaggerResponseHeader] attribute if/when compatible with Swashbuckle.AspNetCore.Filters v10+
    public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var principal = _tokenService.ValidateToken(request.Token);

        if (principal == null)
        {
            return BadRequest(new { error = "Invalid token" });
        }

        Response.Headers.Append("Sunset", "2026-01-01");
        return Ok(new
        {
            valid = true,
            claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToArray(),
            identity = new
            {
                name = principal.Identity?.Name,
                authenticationType = principal.Identity?.AuthenticationType,
                isAuthenticated = principal.Identity?.IsAuthenticated
            }
        });
    }
}

// NOTE: If errors persist, verify the correct namespace and attribute names for Swashbuckle.AspNetCore.Filters v10.0.0. See official docs for any breaking changes.
// Example classes for Swagger examples
#region SwaggerExamples
public class DevTokenResponseExample : IExamplesProvider<DevTokenResponse>
{
    public DevTokenResponse GetExamples() => new DevTokenResponse
    {
        Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        TokenType = "Bearer",
        ExpiresInMinutes = 60,
        UserId = "dev-user",
        Email = "dev@acme.local",
        Roles = new[] { "User", "Developer" },
        Scopes = new[] { "integrations.access", "workflows.access", "admin.access" },
        Instructions = new[]
        {
            "Use this token in the Authorization header as: Bearer {token}",
            "This token is valid for 60 minutes",
            "This endpoint is only available in development environment"
        }
    };
}

public class NotFoundExample : IExamplesProvider<object>
{
    public object GetExamples() => new { error = "Not found" };
}

public class ValidateTokenResponseExample : IExamplesProvider<object>
{
    public object GetExamples() => new
    {
        valid = true,
        claims = new[]
        {
            new { Type = "sub", Value = "dev-user" },
            new { Type = "email", Value = "dev@acme.local" },
            new { Type = "role", Value = "Developer" }
        },
        identity = new
        {
            name = "dev-user",
            authenticationType = "JWT",
            isAuthenticated = true
        }
    };
}

public class InvalidTokenExample : IExamplesProvider<object>
{
    public object GetExamples() => new { error = "Invalid token" };


    #endregion

}

public record DevTokenRequest
{
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string[]? Roles { get; init; }
    public string[]? Scopes { get; init; }
}

public record DevTokenResponse
{
    public required string Token { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresInMinutes { get; init; }
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string[] Roles { get; init; }
    public required string[] Scopes { get; init; }
    public required string[] Instructions { get; init; }
}

public record TokenValidationRequest
{
    public required string Token { get; init; }
}