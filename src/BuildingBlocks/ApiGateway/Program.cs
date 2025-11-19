using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.Middleware;
using BuildingBlocks.Infrastructure.Health;
using BuildingBlocks.Infrastructure.Security;
using Serilog;
using Hellang.Middleware.ProblemDetails;
using Yarp.ReverseProxy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

// Static field for health check tags  
var ReadyTags = new[] { "ready" };

var builder = WebApplication.CreateBuilder(args);

// Platform & cross-cutting services
builder.Services.AddPlatformInfrastructure(builder.Configuration);

// Security: Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthentication", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));

    options.AddPolicy("IntegrationAccess", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "integrations.access"));
});

// Development Services
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<JwtTokenService>();
}// Rate Limiting
builder.Services.AddPlatformRateLimiting(builder.Configuration);

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(
                "https://localhost:3000",  // React dev server
                "https://localhost:4200",  // Angular dev server
                "https://acme-platform.com" // Production domain
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// MVC + Swagger + ProblemDetails
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Acme Platform Gateway API",
        Version = "v1",
        Description = "API Gateway for Acme Platform microservices with authentication and rate limiting"
    });

    // JWT Authentication for Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Enter JWT token to access protected endpoints"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.AddProblemDetails(builder.Services);

// Reverse Proxy with authentication
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck("redis", new TcpPortHealthCheck("localhost", 6379), tags: ReadyTags)
    .AddCheck("rabbitmq", new TcpPortHealthCheck("localhost", 5672), tags: ReadyTags);

var app = builder.Build();

// Security Headers (first in pipeline)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Request Logging
app.UseSerilogRequestLogging();

// Correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// Problem Details
app.UseProblemDetails();

// Development Tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Acme Platform Gateway API v1");
        options.OAuthClientId("swagger-ui");
        options.OAuthAppName("Acme Platform Gateway API");
        options.OAuthUsePkce();
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("DefaultCorsPolicy");

// Rate Limiting
app.UseMiddleware<RateLimitingMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// API Routes
app.MapControllers();

// Reverse Proxy (handles authentication via route policies)
app.MapReverseProxy();

// Health Checks (no authentication required)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

// Security endpoint for token validation
app.MapGet("/auth/validate", (HttpContext context) =>
{
    var user = context.User;
    return Results.Ok(new
    {
        authenticated = user.Identity?.IsAuthenticated ?? false,
        name = user.Identity?.Name,
        claims = user.Claims.Select(c => new { c.Type, c.Value }).ToArray()
    });
}).RequireAuthorization();

app.Run();
