using System;
using System.Threading.Tasks;
using IntegrationHub.Contracts.Events;
using BuildingBlocks.Common.PubSub;

namespace IntegrationHub.Infrastructure.Redis
{
    public class RedisOperationEventPublisher : IntegrationHub.Application.Operations.IOperationEventPublisher
    {
        private readonly IPubSubService _pubSub;
        public RedisOperationEventPublisher(IPubSubService pubSub)
        {
            _pubSub = pubSub;
        }

        public async Task PublishCreatedAsync(Guid operationId, string name)
        {
            var evt = new OperationCreatedEvent(operationId, name);
            var evtJson = System.Text.Json.JsonSerializer.Serialize(evt);
            await _pubSub.PublishAsync($"operation:{operationId}", evtJson);
        }
    }
}
