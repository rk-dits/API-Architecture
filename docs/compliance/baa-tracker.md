# BAA Tracker (Business Associate Agreements)

| Vendor/Provider         | Service/Function             | PHI Exposure | BAA Status | Data Flows/Notes            |
| ----------------------- | ---------------------------- | ------------ | ---------- | --------------------------- |
| Microsoft Azure         | Cloud hosting, DB, Key Vault | Yes          | Signed     | All PHI stored/processed    |
| Seq/ELK                 | Logging/monitoring           | Yes          | Pending    | Audit logs, error logs      |
| RabbitMQ (CloudAMQP)    | Messaging                    | No           | N/A        | No PHI in messages          |
| IntegrationHub Provider | Third-party integration      | Yes/No       | Pending    | Per-provider, see contracts |
| Backup Vendor           | Offsite backup storage       | Yes          | Pending    | PITR backups, DR snapshots  |

## Next Steps

- [ ] Review all vendors for PHI exposure
- [ ] Ensure BAAs are signed before production use
- [ ] Document data flows for each vendor
- [ ] Update status and evidence links regularly

---

_This tracker should be reviewed quarterly and updated as new vendors are onboarded._
