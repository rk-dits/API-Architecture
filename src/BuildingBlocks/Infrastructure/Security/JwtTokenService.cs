using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Service for generating and validating JWT tokens - primarily for development and testing
/// </summary>
public class JwtTokenService
{
    private readonly JwtAuthenticationOptions _options;
    private readonly SymmetricSecurityKey _key;
    private readonly SigningCredentials _credentials;

    public JwtTokenService(IOptions<JwtAuthenticationOptions> options)
    {
        _options = options.Value;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        _credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// Generates a JWT token with the specified claims
    /// </summary>
    public string GenerateToken(string userId, string email, IEnumerable<string>? roles = null, IEnumerable<string>? scopes = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add roles
        if (roles != null)
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        // Add scopes
        if (scopes != null)
        {
            claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a token for development with common scopes
    /// </summary>
    public string GenerateDevToken(string userId = "dev-user", string email = "dev@acme.local")
    {
        var roles = new[] { "User", "Developer" };
        var scopes = new[] { "integrations.access", "workflows.access", "admin.access" };

        return GenerateToken(userId, email, roles, scopes);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _options.ValidateIssuerSigningKey,
                IssuerSigningKey = _key,
                ValidateIssuer = _options.ValidateIssuer,
                ValidIssuer = _options.Issuer,
                ValidateAudience = _options.ValidateAudience,
                ValidAudience = _options.Audience,
                ValidateLifetime = _options.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}