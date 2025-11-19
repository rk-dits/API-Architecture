using StackExchange.Redis;
using System;
using System.Threading.Tasks;

using BuildingBlocks.Common.PubSub;

namespace IntegrationHub.Infrastructure.Redis
{
    public class RedisPubSubService : IPubSubService, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly ISubscriber _subscriber;
        private bool _disposed = false;

        public RedisPubSubService(string configuration)
        {
            _redis = ConnectionMultiplexer.Connect(configuration);
            _subscriber = _redis.GetSubscriber();
        }

        public async Task PublishAsync(string channel, string message)
        {
            await _subscriber.PublishAsync(channel, message);
        }

        public async Task SubscribeAsync(string channel, Action<string> handler)
        {
            await _subscriber.SubscribeAsync(channel, (ch, msg) => handler(msg));
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
                _redis?.Dispose();
                _disposed = true;
            }
        }
    }
}
