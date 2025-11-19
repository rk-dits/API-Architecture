# Risk Analysis & Threat Model Worksheet

## Top Risks & Mitigations

| Risk ID | Description                               | Likelihood | Impact | Mitigation/Control                        | Owner         |
| ------- | ----------------------------------------- | ---------- | ------ | ----------------------------------------- | ------------- |
| R1      | Unauthorized access to PHI                | Medium     | High   | RBAC, MFA, audit logging, least privilege | Security Lead |
| R2      | Data breach via third-party integration   | Low        | High   | IntegrationHub boundary, BAA, token mgmt  | CTO           |
| R3      | Data loss (DB corruption, backup failure) | Low        | High   | PITR backups, DR runbooks, restore tests  | Infra Lead    |
| R4      | Incomplete audit trail                    | Medium     | Med    | Immutable logs, SIEM forwarding           | DevOps Lead   |
| R5      | PHI in logs/telemetry                     | Medium     | High   | Log redaction, masking, code review       | Dev Lead      |
| R6      | Vendor lock-in (cloud, DB, messaging)     | Low        | Med    | Abstraction layers, contract tests        | Architect     |
| R7      | Unpatched vulnerabilities                 | Medium     | High   | SAST/DAST, container scanning, patch mgmt | DevOps Lead   |

## Threat Model Summary

- **Assets:** PHI, PII, credentials, tokens, audit logs
- **Adversaries:** External attackers, malicious insiders, compromised vendors
- **Attack Vectors:** API endpoints, integrations, misconfigurations, supply chain
- **Controls:**
  - OAuth2/JWT, RBAC, audit logging, encryption, rate limiting
  - IntegrationHub boundary for all 3P traffic
  - Immutable audit logs, SIEM forwarding
  - Regular access reviews, backup/restore tests

## Next Steps

- [ ] Review and update risks quarterly
- [ ] Map mitigations to repo controls and evidence
- [ ] Link to incident response and DR runbooks

---

_This worksheet should be reviewed and updated as the platform evolves._
