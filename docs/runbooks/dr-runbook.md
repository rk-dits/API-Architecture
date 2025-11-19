# Disaster Recovery (DR) Runbook

## 1. DR Plan Overview

- [ ] Define RPO (Recovery Point Objective) and RTO (Recovery Time Objective)
- [ ] Document critical systems and dependencies
- [ ] Identify DR team and contact info

## 2. Backup & Restore Procedures

- [ ] Verify backup schedule and retention
- [ ] Test restore procedures quarterly
- [ ] Document restore steps for PostgreSQL, Redis, RabbitMQ
- [ ] Validate data integrity after restore

## 3. Failover & Recovery

- [ ] Document failover process (manual/automated)
- [ ] Test failover to secondary region/environment
- [ ] Validate application health and connectivity

## 4. Communication & Escalation

- [ ] Notify DR team and stakeholders
- [ ] Use communication templates for status updates
- [ ] Document all actions and evidence

## 5. Review & Improvement

- [ ] Conduct post-DR test review
- [ ] Update runbook and DR plan as needed

## Evidence Template

| Date       | System     | Test Type | Result  | Evidence Link/Attachment |
| ---------- | ---------- | --------- | ------- | ------------------------ |
| YYYY-MM-DD | PostgreSQL | Restore   | Success | restore-log.txt          |
| YYYY-MM-DD | Redis      | Failover  | Success | screenshot.png           |

---

_This runbook should be reviewed and tested at least annually, and after any major DR event._
