# GDPR Readiness Checklist (Starter)

> This checklist is a living document to help track GDPR readiness for the Acme Platform API. It does not confer compliance, but helps organize evidence and tasks.

## Data Protection Principles

- [ ] Lawfulness, fairness, transparency: Privacy policy published and reviewed
- [ ] Purpose limitation: Data collected for specified, explicit purposes
- [ ] Data minimization: Only necessary data collected and processed
- [ ] Accuracy: Data kept up to date
- [ ] Storage limitation: Data retention and disposal policy enforced
- [ ] Integrity and confidentiality: Security controls in place (encryption, access control)

## Data Subject Rights

- [ ] Right to access: Process for data subject access requests (DSAR)
- [ ] Right to rectification: Process for correcting data
- [ ] Right to erasure: Process for deleting data
- [ ] Right to restrict processing: Mechanism to restrict data use
- [ ] Right to data portability: Export data in common format
- [ ] Right to object: Mechanism to object to processing

## Security Measures

- [ ] Data encryption at rest and in transit
- [ ] Access controls and audit logging
- [ ] Data breach notification process
- [ ] Data protection impact assessment (DPIA) for new features

## Platform Implementation Hooks (To Map)

| Requirement         | Planned Mechanism                           | Status  |
| ------------------- | ------------------------------------------- | ------- |
| Data Subject Rights | API endpoints for DSAR, correction, erasure | Pending |
| Data Encryption     | PostgreSQL + application-layer (future)     | Pending |
| Audit Logging       | Serilog + centralized sinks (Seq/ELK)       | Pending |
| Breach Notification | Incident response workflow                  | Pending |
| Data Minimization   | Domain model review + privacy by design     | Pending |

## Next Steps

1. Assign control owners and evidence collection tasks.
2. Map platform features to GDPR controls.
3. Add automated compliance test suite.

_Revision: initial scaffold. Expand with control owners, evidence links, and review cadence._
