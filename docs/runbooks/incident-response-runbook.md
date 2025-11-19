# Incident Response & Breach Notification Runbook

## 1. Incident Detection

- [ ] Monitor logs, alerts, and SIEM for suspicious activity
- [ ] Triage and classify incident severity

## 2. Containment & Eradication

- [ ] Isolate affected systems/accounts
- [ ] Block malicious access or activity
- [ ] Remove malware or unauthorized changes

## 3. Notification & Communication

- [ ] Notify incident response team and management
- [ ] Prepare communication for affected users (if PHI/PII involved)
- [ ] Notify regulators as required (HIPAA: within 60 days)
- [ ] Use communication templates for internal/external updates

## 4. Investigation & Evidence Collection

- [ ] Collect logs, audit trails, and system snapshots
- [ ] Document timeline and actions taken
- [ ] Preserve evidence for legal/regulatory review

## 5. Recovery & Lessons Learned

- [ ] Restore systems from clean backups
- [ ] Validate system integrity and security controls
- [ ] Conduct post-mortem and update runbooks

## RACI Table

| Role             | Responsible | Accountable | Consulted | Informed |
| ---------------- | ----------- | ----------- | --------- | -------- |
| Security Lead    | X           | X           |           | X        |
| DevOps Lead      | X           |             | X         | X        |
| Legal/Compliance |             | X           | X         | X        |
| Management       |             | X           |           | X        |

## Evidence Template

| Date       | Incident ID | Action Taken        | Evidence Link/Attachment |
| ---------- | ----------- | ------------------- | ------------------------ |
| YYYY-MM-DD | INC-001     | Contained, notified | screenshot.png           |

---

_This runbook should be reviewed and tested at least annually, and after any major incident._
