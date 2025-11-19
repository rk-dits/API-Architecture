# FinOps Guardrails (Sample)

## Environment Budgets

- Set monthly spend limits per environment (dev, staging, prod).
- Use Azure Cost Management budgets and alerts.

## Example Azure CLI Budget

```sh
az consumption budget create \
  --amount 500 \
  --category cost \
  --name dev-budget \
  --resource-group Acme-Dev \
  --time-grain monthly \
  --start-date 2025-01-01 \
  --end-date 2026-01-01 \
  --notification enabled=true,operator=GreaterThan,threshold=80,contactEmails=finops@acme.com
```

## CI Cost Guardrails

- Add infra plan diffs to PRs (e.g., Terraform plan, Bicep what-if).
- Fail CI if projected cost delta exceeds threshold.

## Spend Alarms

- Enable spend alarms in Azure and integrate with on-call/Slack.

---

_Track, alert, and control cloud spend to avoid overruns._
