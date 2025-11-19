# Sample Provider Adapter: FooHealthAdapter

## Purpose

Integrates with FooHealth EHR system using HL7 FHIR R4 REST API.

## Features

- Maps FHIR Patient, Encounter, and Observation resources
- Handles OAuth2/SMART on FHIR authentication
- Implements retry, timeout, and circuit breaker policies
- Redacts/masks PHI in logs
- Publishes audit events for all PHI access

## Example Usage (pseudo-C#)

```csharp
public class FooHealthAdapter : IProviderAdapter
{
    public async Task<FhirPatient> GetPatientAsync(string id)
    {
        // Call FooHealth FHIR API
        // Apply retry/timeout/circuit breaker
        // Map response to FhirPatient
        // Publish audit event
        // Redact PHI in logs
    }
}
```

---

_This adapter is referenced in IntegrationHub for healthcare integrations._
