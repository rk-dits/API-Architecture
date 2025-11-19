# SOC 2 Readiness Checklist (Starter)

> This checklist is a living document to help track SOC 2 readiness for the Acme Platform API. It does not confer compliance, but helps organize evidence and tasks.

## Security (Common Criteria)

- [ ] Access controls: Role-based access, least privilege, periodic review
- [ ] Authentication: MFA for all admin access, SSO for users
- [ ] Audit logging: Centralized, immutable, and regularly reviewed
- [ ] Change management: Documented process, approvals, and rollback
- [ ] Incident response: Plan documented, tested, and reviewed
- [ ] Vendor management: Due diligence and risk assessment
- [ ] Encryption: Data at rest and in transit
- [ ] Vulnerability management: Regular scans and patching

## Availability

- [ ] Disaster recovery plan: Documented and tested
- [ ] Backup procedures: Regular, tested, and offsite
- [ ] Capacity planning: Monitored and forecasted

## Confidentiality

- [ ] Data classification: Inventory and label sensitive data
- [ ] Encryption: Strong encryption for confidential data
- [ ] Data retention and disposal: Policy and enforcement

## Processing Integrity

- [ ] Input validation: Automated and tested
- [ ] Error handling: Logged and monitored
- [ ] Data reconciliation: Regular checks and alerts

## Privacy

- [ ] Privacy policy: Published and reviewed
- [ ] Data subject rights: Processes for access, correction, deletion
- [ ] Consent management: Tracked and auditable

## Platform Implementation Hooks (To Map)

| Requirement       | Planned Mechanism                         | Status  |
| ----------------- | ----------------------------------------- | ------- |
| Audit Logging     | Serilog + centralized sinks (Seq/ELK)     | Pending |
| Access Control    | ASP.NET Core authZ policies + role claims | Pending |
| Data Encryption   | PostgreSQL + application-layer (future)   | Pending |
| DR & Backup       | Postgres WAL archiving + infra scripts    | Pending |
| Vendor Management | Vendor review checklist + contracts       | Pending |

## Next Steps

1. Assign control owners and evidence collection tasks.
2. Map platform features to SOC 2 controls.
3. Add automated compliance test suite.

_Revision: initial scaffold. Expand with control owners, evidence links, and review cadence._
