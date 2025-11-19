using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CoreWorkflowService.Infrastructure.Persistence;
using CoreWorkflowService.Infrastructure.Persistence.Entity;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Messaging.Abstractions;

namespace CoreWorkflowService.Infrastructure.Messaging;

public class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;
    public OutboxDispatcherHostedService(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CoreWorkflowServiceDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var now = DateTime.UtcNow;
                var pending = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null && (x.NextAttemptAt == null || x.NextAttemptAt <= now) && x.Attempts < 5)
                    .OrderBy(x => x.OccurredAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var msg in pending)
                {
                    try
                    {
                        var eventType = ResolveEventType(msg.Type);
                        if (eventType is null)
                        {
                            _logger.LogWarning("Outbox type not resolved: {Type}", msg.Type);
                            msg.Error = $"Type not found: {msg.Type}";
                            msg.ProcessedAt = DateTime.UtcNow;
                            continue;
                        }

                        var evt = System.Text.Json.JsonSerializer.Deserialize(msg.Payload, eventType);
                        if (evt is null)
                        {
                            _logger.LogWarning("Outbox payload deserialization failed for {Type} {Id}", msg.Type, msg.Id);
                            msg.Error = "Deserialization failed";
                            msg.ProcessedAt = DateTime.UtcNow;
                            continue;
                        }

                        var method = typeof(IEventPublisher).GetMethod(nameof(IEventPublisher.PublishAsync));
                        var generic = method!.MakeGenericMethod(eventType);
                        await (Task)generic.Invoke(publisher, new object?[] { evt, stoppingToken })!;

                        _logger.LogInformation("Outbox dispatched {Type} {Id}", msg.Type, msg.Id);
                        msg.ProcessedAt = DateTime.UtcNow;
                        msg.Error = null;
                    }
                    catch (Exception ex)
                    {
                        msg.Attempts += 1;
                        var delaySeconds = Math.Pow(2, Math.Min(msg.Attempts, 6));
                        msg.NextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);
                        _logger.LogWarning(ex, "Outbox dispatch error attempt {Attempts} for {Type} {Id}, next at {NextAttemptAt}", msg.Attempts, msg.Type, msg.Id, msg.NextAttemptAt);
                        msg.Error = ex.Message;
                        if (msg.Attempts >= 5)
                        {
                            msg.ProcessedAt = DateTime.UtcNow; // give up after max attempts
                        }
                    }
                }

                if (pending.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Outbox dispatcher encountered an error");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private static Type? ResolveEventType(string typeName)
    {
        var t = Type.GetType(typeName, throwOnError: false);
        if (t != null) return t;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            t = asm.GetType(typeName, throwOnError: false);
            if (t != null) return t;
        }
        return null;
    }
}
