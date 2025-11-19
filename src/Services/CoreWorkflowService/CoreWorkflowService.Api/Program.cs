using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.Middleware;
using BuildingBlocks.Infrastructure.Security;
using BuildingBlocks.Infrastructure.Health;
using CoreWorkflowService.Infrastructure.DependencyInjection;
using CoreWorkflowService.Infrastructure.Persistence;
using Serilog;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Platform & cross-cutting services
builder.Services.AddPlatformInfrastructure(builder.Configuration);

// Service-specific configuration
builder.Services.AddCoreWorkflowServiceModule(builder.Configuration);

// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthentication", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("WorkflowAccess", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "workflows.access"));
});

// Rate Limiting
builder.Services.AddPlatformRateLimiting(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CoreWorkflowServiceDbContext>(name: "db", tags: new[] { "ready" })
    .AddCheck("redis", new TcpPortHealthCheck("localhost", 6379), tags: new[] { "ready" })
    .AddCheck("rabbitmq", new TcpPortHealthCheck("localhost", 5672), tags: new[] { "ready" });

// API Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CoreWorkflowService API",
        Version = "v1",
        Description = "API for managing workflow cases and operations"
    });

    // JWT Authentication for Swagger
    options.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT token to access protected endpoints"
    });

    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.AddProblemDetails(builder.Services);

var app = builder.Build();

// Security Headers
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreWorkflowService API v1");
        options.OAuthClientId("core-workflow-swagger");
        options.OAuthAppName("CoreWorkflowService API");
        options.OAuthUsePkce();
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Rate Limiting
app.UseMiddleware<RateLimitingMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// API Routes (protected by default)
app.MapControllers()
    .RequireAuthorization("RequireAuthentication");

// Health Checks (public access)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.Run();
