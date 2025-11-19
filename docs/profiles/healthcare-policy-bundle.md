# Healthcare Profile — Policy Bundle

## Timeouts

- Per-provider: 8 seconds
- Overall operation SLA: 15 seconds

## Retries

- 2 retries with jittered exponential backoff (max 3 attempts)

## Circuit Breaker

- Open after 5 consecutive failures, reset after 60 seconds

## Bulkhead

- Max 10 concurrent requests per provider

## Security

- OAuth2/SMART on FHIR scopes
- Field-level encryption for PHI
- Audit logging for all PHI access

## Compliance

- HIPAA/HITECH, BAAs, audit trails, PHI minimization

---

_This bundle is referenced by IntegrationHub adapters for healthcare integrations._
