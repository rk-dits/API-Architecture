# Sample Provider Adapter: BarLogisticsAdapter

## Purpose

Integrates with BarLogistics carrier using EDI X12 214 messages and REST API.

## Features

- Maps EDI 214 status messages to domain events
- Handles API key authentication
- Implements retry, timeout, and circuit breaker policies
- Tamper-evident logging for all status updates
- Publishes audit events for all chain of custody changes

## Example Usage (pseudo-C#)

```csharp
public class BarLogisticsAdapter : IProviderAdapter
{
    public async Task<ShipmentStatus> GetStatusAsync(string trackingId)
    {
        // Call BarLogistics REST API or receive EDI 214
        // Apply retry/timeout/circuit breaker
        // Map response to ShipmentStatus
        // Publish audit event
        // Tamper-evident log
    }
}
```

---

_This adapter is referenced in IntegrationHub for logistics integrations._
