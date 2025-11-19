# ADR: Healthcare Profile — HL7 FHIR Integration

## Context

The Acme Platform must support healthcare integrations using HL7 FHIR R4/R5 REST APIs, HL7 v2 messaging, and DICOM pointer models. Compliance with HIPAA/HITECH and support for PHI handling, audit trails, and BAAs is required.

## Decision

- Use HL7 FHIR R4 RESTful APIs for patient, encounter, and observation resources.
- HL7 v2 (ADT/ORM/ORU) supported via IntegrationHub adapters.
- DICOM handled as metadata + signed URLs (no binary storage in OLTP).
- SMART on FHIR OAuth flows for patient access and consent management.
- PHI fields classified and encrypted at rest and in transit.
- Immutable audit logs for all PHI access.

## Consequences

- Enables interoperability with EHRs and health systems.
- Ensures HIPAA compliance and auditability.
- Requires ongoing maintenance of FHIR profiles and terminology bindings.

---

_This ADR is part of the Healthcare domain profile._
