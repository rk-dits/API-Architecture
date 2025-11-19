# API Security & Compliance Overview

This document summarizes the most common and relevant compliance and security controls implemented or planned for the Acme Platform API.

## Security Foundation (see `security-foundation.md`)

- JWT/OAuth2 authentication
- Rate limiting
- Security headers
- API Gateway security offloading
- Development token generation

## Compliance Checklists

- [HIPAA Readiness Checklist](compliance/hipaa-checklist.md)
- [SOC 2 Readiness Checklist](compliance/soc2-checklist.md)
- [GDPR Readiness Checklist](compliance/gdpr-checklist.md)

## Implementation Hooks

| Control Area        | Mechanism/Status                                 |
| ------------------- | ------------------------------------------------ |
| Authentication      | JWT, OAuth2, ASP.NET Core policies (implemented) |
| Authorization       | Role/Scope-based, policies (implemented)         |
| Audit Logging       | Serilog, structured sinks (planned)              |
| Rate Limiting       | In-memory, per-user/IP/client (implemented)      |
| Data Encryption     | PostgreSQL, app-layer (planned)                  |
| Backup & DR         | WAL archiving, infra scripts (planned)           |
| Vendor Management   | Checklist, contracts (planned)                   |
| Data Subject Rights | API endpoints (planned)                          |
| Breach Notification | Incident workflow (planned)                      |

## Next Steps

- Assign control owners for each compliance area
- Expand checklists with evidence and review cadence
- Implement automated compliance test suite
- Map platform features to each compliance control

---

_This document is a living summary. For detailed requirements, see the individual checklists and security foundation documentation._
