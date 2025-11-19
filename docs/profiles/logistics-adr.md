# Logistics Profile — ADR: EDI & Track/Trace

## Context

The Acme Platform must support logistics integrations using EDI X12/EDIFACT, GS1 identifiers, and ISO 8601/UTC time rigor. Capabilities include track & trace, carrier rate-shopping, and geofencing.

## Decision

- Use EDI X12/EDIFACT for core logistics messaging (e.g., 940/945/856/214).
- GS1 identifiers for shipments, locations, and assets.
- All times in ISO 8601/UTC.
- Track & trace events modeled as domain events and exposed via API.
- IoT ingestion endpoints for device telemetry, with burst/backpressure controls.
- Tamper-evident chain of custody for sensitive data.

## Consequences

- Enables interoperability with carriers and logistics partners.
- Ensures data integrity and traceability.
- Requires ongoing maintenance of EDI mappings and GS1 standards.

---

_This ADR is part of the Logistics domain profile._
