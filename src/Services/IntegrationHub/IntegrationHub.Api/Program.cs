using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.Middleware;
using BuildingBlocks.Infrastructure.Security;
using BuildingBlocks.Infrastructure.Health;
using IntegrationHub.Infrastructure.DependencyInjection;
using IntegrationHub.Infrastructure.Persistence;
using IntegrationHub.Contracts.Events;
using Serilog;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;


using IntegrationHub.Infrastructure.Persistence.Entity;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Platform & cross-cutting services
builder.Services.AddPlatformInfrastructure(builder.Configuration);

// Service-specific configuration
builder.Services.AddIntegrationHubModule(builder.Configuration);


// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger/OpenAPI configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IntegrationHub API",
        Version = "v1",
        Description = "API for managing third-party integrations and operations"
    });

    // Enable Swagger annotations
    options.EnableAnnotations();

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

var app = builder.Build();

// Security Headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Request Logging (temporarily disabled due to DiagnosticContext resolution issue)
// app.UseSerilogRequestLogging();

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IntegrationHub API v1");
        options.OAuthClientId("integration-hub-swagger");
        options.OAuthAppName("IntegrationHub API");
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

// SignalR Hub endpoint for real-time operations
app.MapHub<IntegrationHub.Api.Hubs.RealTimeOperationsHub>("/realtime/operations");

// Health Checks (public access)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.Run();
