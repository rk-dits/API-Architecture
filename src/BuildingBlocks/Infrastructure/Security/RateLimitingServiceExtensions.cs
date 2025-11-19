using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Configuration options for rate limiting policies
/// </summary>
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public GlobalPolicy Global { get; set; } = new();
    public ApiPolicy Api { get; set; } = new();
    public IntegrationPolicy Integration { get; set; } = new();

    public class GlobalPolicy
    {
        public int PermitLimit { get; set; } = 100;
        public int WindowSizeMinutes { get; set; } = 1;
    }

    public class ApiPolicy
    {
        public int PermitLimit { get; set; } = 60;
        public int WindowSizeMinutes { get; set; } = 1;
    }

    public class IntegrationPolicy
    {
        public int PermitLimit { get; set; } = 30;
        public int WindowSizeMinutes { get; set; } = 1;
    }
}

/// <summary>
/// Simple rate limiting service using in-memory sliding window
/// </summary>
public interface IRateLimitingService
{
    Task<bool> IsRequestAllowedAsync(string identifier, string policy = "Global");
}

public class InMemoryRateLimitingService : IRateLimitingService, IDisposable
{
    private readonly RateLimitingOptions _options;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed = false;

    public InMemoryRateLimitingService(Microsoft.Extensions.Options.IOptions<RateLimitingOptions> options)
    {
        _options = options.Value;
    }

    public async Task<bool> IsRequestAllowedAsync(string identifier, string policy = "Global")
    {
        await _semaphore.WaitAsync();
        try
        {
            var (limit, windowMinutes) = policy switch
            {
                "ApiPolicy" => (_options.Api.PermitLimit, _options.Api.WindowSizeMinutes),
                "IntegrationPolicy" => (_options.Integration.PermitLimit, _options.Integration.WindowSizeMinutes),
                _ => (_options.Global.PermitLimit, _options.Global.WindowSizeMinutes)
            };

            var key = $"{policy}:{identifier}";
            var now = DateTime.UtcNow;
            var window = TimeSpan.FromMinutes(windowMinutes);

            if (!_requests.TryGetValue(key, out var requestTimes))
            {
                requestTimes = new Queue<DateTime>();
                _requests[key] = requestTimes;
            }

            // Remove expired requests
            while (requestTimes.Count > 0 && now - requestTimes.Peek() > window)
            {
                requestTimes.Dequeue();
            }

            // Check if limit exceeded
            if (requestTimes.Count >= limit)
            {
                return false;
            }

            // Add current request
            requestTimes.Enqueue(now);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _semaphore?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Rate limiting middleware for HTTP requests
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;

    public RateLimitingMiddleware(RequestDelegate next, IRateLimitingService rateLimitingService)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var identifier = GetUserIdentifier(context);
        var policy = GetPolicyForPath(context.Request.Path);

        var isAllowed = await _rateLimitingService.IsRequestAllowedAsync(identifier, policy);

        if (!isAllowed)
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = "https://httpstatuses.com/429",
                title = "Too Many Requests",
                status = 429,
                detail = "Rate limit exceeded. Please retry after some time.",
                retryAfter = 60
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problem));
            return;
        }

        await _next(context);
    }

    private static string GetUserIdentifier(HttpContext context)
    {
        // Prefer authenticated user ID, fallback to IP address
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var clientId = context.Request.Headers["X-Client-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientId))
        {
            return $"client:{clientId}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private static string GetPolicyForPath(PathString path)
    {
        if (path.StartsWithSegments("/api/v1/integrations"))
            return "IntegrationPolicy";

        if (path.StartsWithSegments("/api"))
            return "ApiPolicy";

        return "Global";
    }
}

public static class RateLimitingServiceExtensions
{
    /// <summary>
    /// Configures rate limiting services for the platform
    /// </summary>
    public static IServiceCollection AddPlatformRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(
            configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddSingleton<IRateLimitingService, InMemoryRateLimitingService>();

        return services;
    }
}