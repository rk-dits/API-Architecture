using System;
using System.Threading.Tasks;

namespace BuildingBlocks.Common.PubSub
{
    public interface IPubSubService
    {
        Task PublishAsync(string channel, string message);
        Task SubscribeAsync(string channel, Action<string> handler);
    }
}
