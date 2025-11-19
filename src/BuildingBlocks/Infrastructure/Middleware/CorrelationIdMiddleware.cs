using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace BuildingBlocks.Infrastructure.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationHeader] = correlationId;
        }

        context.Response.Headers[CorrelationHeader] = correlationId;
        var userId = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity!.Name : "anonymous";
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        using (LogContext.PushProperty("RemoteIP", remoteIp ?? "unknown"))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("HttpMethod", context.Request.Method))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdExtensions
{
    // Removed extension to avoid missing ASP.NET reference; APIs use UseMiddleware directly.
}
