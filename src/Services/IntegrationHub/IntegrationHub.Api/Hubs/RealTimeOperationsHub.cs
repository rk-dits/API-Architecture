using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using BuildingBlocks.Common.PubSub;

namespace IntegrationHub.Api.Hubs
{
    public class RealTimeOperationsHub : Hub
    {
        private readonly IPubSubService _pubSub;
        public RealTimeOperationsHub(IPubSubService pubSub)
        {
            _pubSub = pubSub;
        }

        // Called by clients to join a group for a specific operation
        public async Task JoinOperationGroup(string operationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"operation:{operationId}");

            // Subscribe to Redis pub/sub for this operation and broadcast to group
            _ = _pubSub.SubscribeAsync($"operation:{operationId}", async msg =>
            {
                await Clients.Group($"operation:{operationId}").SendAsync("ReceiveOperationEvent", msg);
            });
        }

        // Called by clients to leave a group
        public async Task LeaveOperationGroup(string operationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"operation:{operationId}");
        }
    }
}
