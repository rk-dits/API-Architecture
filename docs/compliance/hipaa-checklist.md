# HIPAA Readiness Checklist (Starter)

> This starter checklist is a living document. It does not confer compliance; it helps track readiness tasks.

## Administrative Safeguards

- [ ] Security Management Process: risk analysis documented
- [ ] Workforce Training: initial + annual refresher
- [ ] Information Access Management: role-based access matrix defined
- [ ] Contingency Plan: backup & disaster recovery procedures drafted
- [ ] Incident Response: breach notification workflow established

## Physical Safeguards

- [ ] Facility Access Controls: hosting provider SOC2 report collected
- [ ] Workstation Security: hardening baseline documented
- [ ] Device / Media Controls: encryption policy for removable media

## Technical Safeguards

- [ ] Access Control: unique user IDs, MFA enforced
- [ ] Audit Controls: structured logging & retention policy (e.g., 6 years)
- [ ] Integrity: hashing strategy for PHI payloads at rest/in transit
- [ ] Person/Entity Authentication: identity provider integration (OIDC)
- [ ] Transmission Security: TLS 1.2+ enforced; HSTS preloaded

## Policies & Procedures

- [ ] Acceptable Use Policy
- [ ] Data Retention & Disposal Policy
- [ ] Business Associate Agreements (BAAs) cataloged
- [ ] Encryption Key Management SOP
- [ ] Third-Party Vendor Review process

## Platform Implementation Hooks (To Map)

| Requirement             | Planned Mechanism                                  | Status  |
| ----------------------- | -------------------------------------------------- | ------- |
| Audit Logging           | Serilog + structured sinks (Seq / ELK)             | Pending |
| Access Control          | ASP.NET Core authZ policies + role claims          | Pending |
| Data Encryption at Rest | PostgreSQL disk-level + application-layer (future) | Pending |
| Transport Encryption    | HTTPS enforced by reverse proxy                    | Partial |
| Backup & DR             | Postgres WAL archiving + infra scripts             | Pending |

## PHI Data Classification

- [ ] Inventory PHI data fields
- [ ] Tag domain models containing PHI (attributes or metadata)
- [ ] Define redaction rules for logs (Serilog enricher)

## Next Steps

1. Add compliance annotations (e.g., `[ContainsPHI]`) to domain entities.
2. Implement audit trail outbox for create/update events.
3. Introduce encryption service abstraction for sensitive fields.
4. Add automated policy compliance test suite.

_Revision: initial scaffold. Expand with control owners, evidence links, and review cadence._
