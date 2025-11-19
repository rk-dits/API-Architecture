# PHI Data Map & Masking Rules

## PHI Field Inventory

| Service/Domain      | Field Name    | Data Type | PHI? | Masking/Redaction | Encryption | Notes                  |
| ------------------- | ------------- | --------- | ---- | ----------------- | ---------- | ---------------------- |
| CoreWorkflowService | PatientName   | string    | Yes  | Mask (first char) | Yes        | Field-level encryption |
| CoreWorkflowService | SSN           | string    | Yes  | Full redact       | Yes        | Never logged           |
| IntegrationHub      | ProviderNPI   | string    | Yes  | Mask (last 4)     | Yes        |                        |
| IntegrationHub      | DiagnosisCode | string    | Yes  | None              | No         | Not logged             |

## Masking/Redaction Rules

- **Mask (first char):** Only show first character, rest replaced with \*
- **Full redact:** Replace entire value with [REDACTED]
- **Mask (last 4):** Show all but last 4 characters as \*
- **None:** No masking, but field not logged

## Logging & Telemetry

- No PHI fields should be included in logs or telemetry
- Use Serilog enrichers to redact/mask PHI fields automatically

## Next Steps

- [ ] Review and update PHI field inventory for all services
- [ ] Implement masking/redaction in logging pipeline
- [ ] Review field-level encryption for all PHI fields

---

_This map should be reviewed and updated as new PHI fields are added or changed._
