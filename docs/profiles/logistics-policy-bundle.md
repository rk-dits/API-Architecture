# Logistics Profile — Policy Bundle

## Timeouts

- Per-provider: 5 seconds
- Overall operation SLA: 12 seconds

## Retries

- 1 retry with exponential backoff (max 2 attempts)

## Circuit Breaker

- Open after 3 consecutive failures, reset after 30 seconds

## Bulkhead

- Max 5 concurrent requests per provider

## Security

- OAuth2 client credentials or API keys
- Tamper-evident audit logging for chain of custody

## Compliance

- Data minimization, location privacy, chain of custody

---

_This bundle is referenced by IntegrationHub adapters for logistics integrations._
