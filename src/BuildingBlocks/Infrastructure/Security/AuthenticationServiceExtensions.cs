using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Configuration options for JWT authentication
/// </summary>
public class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public int ClockSkewSeconds { get; set; } = 300; // 5 minutes
}

/// <summary>
/// OAuth2/OIDC configuration options for Entra ID integration
/// </summary>
public class OAuthOptions
{
    public const string SectionName = "Authentication:OAuth";

    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public bool RequireHttpsMetadata { get; set; } = true;
    public string MetadataAddress { get; set; } = string.Empty;
}

public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication with flexible issuer support
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtAuthenticationOptions.SectionName)
            .Get<JwtAuthenticationOptions>() ?? new JwtAuthenticationOptions();

        services.Configure<JwtAuthenticationOptions>(
            configuration.GetSection(JwtAuthenticationOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                    ValidateIssuer = jwtOptions.ValidateIssuer,
                    ValidateAudience = jwtOptions.ValidateAudience,
                    ValidateLifetime = jwtOptions.ValidateLifetime,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds),
                    IssuerSigningKey = !string.IsNullOrEmpty(jwtOptions.SecretKey)
                        ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                        : null
                };

                // Handle authentication failures
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/problem+json";

                        var problem = new
                        {
                            type = "https://httpstatuses.com/401",
                            title = "Unauthorized",
                            status = 401,
                            detail = "Valid authentication token required"
                        };

                        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problem));
                    }
                };
            });

        return services;
    }

    /// <summary>
    /// Configures OAuth2/OIDC authentication for Entra ID integration
    /// </summary>
    public static IServiceCollection AddOAuthAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var oauthOptions = configuration.GetSection(OAuthOptions.SectionName)
            .Get<OAuthOptions>() ?? new OAuthOptions();

        services.Configure<OAuthOptions>(
            configuration.GetSection(OAuthOptions.SectionName));

        if (!string.IsNullOrEmpty(oauthOptions.Authority))
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = oauthOptions.Authority;
                    options.Audience = oauthOptions.ClientId;
                    options.RequireHttpsMetadata = oauthOptions.RequireHttpsMetadata;

                    if (!string.IsNullOrEmpty(oauthOptions.MetadataAddress))
                    {
                        options.MetadataAddress = oauthOptions.MetadataAddress;
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });
        }

        return services;
    }
}