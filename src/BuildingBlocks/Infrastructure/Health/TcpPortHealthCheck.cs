using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Infrastructure.Health;

public class TcpPortHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly int _port;

    public TcpPortHealthCheck(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(_host, _port);
            using (cancellationToken.Register(() => client.Dispose()))
            {
                await connectTask.ConfigureAwait(false);
            }
            return HealthCheckResult.Healthy($"TCP {_host}:{_port} reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"TCP {_host}:{_port} unreachable", ex);
        }
    }
}