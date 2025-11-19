# Backup Restore Test Checklist

## Checklist

- [ ] Identify backup to restore (date, type, system)
- [ ] Document RPO/RTO objectives for the restore
- [ ] Perform restore in test environment (do not overwrite production)
- [ ] Validate data integrity and completeness
- [ ] Document time to restore and any issues
- [ ] Capture evidence (logs, screenshots, reports)
- [ ] Review and sign off by responsible parties

## Evidence Template

| Date       | System     | Backup Type | RPO/RTO | Result  | Evidence Link/Attachment |
| ---------- | ---------- | ----------- | ------- | ------- | ------------------------ |
| YYYY-MM-DD | PostgreSQL | PITR        | 1h/4h   | Success | restore-log.txt          |
| YYYY-MM-DD | Redis      | Snapshot    | 15m/1h  | Success | screenshot.png           |

## Next Steps

- [ ] Schedule next restore test (at least quarterly)
- [ ] File evidence in compliance repository
- [ ] Update restore test log

---

_This checklist and template should be used for every backup restore test and retained for audit evidence._
