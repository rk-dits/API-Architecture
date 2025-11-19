using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using IntegrationHub.Contracts.Events;

namespace IntegrationHub.Infrastructure.Messaging.Consumers;

public class OperationCreatedEventConsumer : IConsumer<OperationCreatedEvent>
{
    private readonly ILogger<OperationCreatedEventConsumer> _logger;
    public OperationCreatedEventConsumer(ILogger<OperationCreatedEventConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<OperationCreatedEvent> context)
    {
        _logger.LogInformation("Consumed OperationCreatedEvent OperationId={OperationId} Name={Name}", context.Message.OperationId, context.Message.Name);
        return Task.CompletedTask;
    }
}
