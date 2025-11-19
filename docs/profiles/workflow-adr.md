# Workflow Automation Profile — ADR: BPMN & Connectors

## Context

The Acme Platform must support workflow automation using BPMN 2.0 process models, work item queueing, SLAs, and SaaS connectors (Zendesk, Jira, O365, Slack) via IntegrationHub.

## Decision

- Use BPMN 2.0 for process modeling and documentation.
- Optional integration with workflow engines (e.g., Camunda, Temporal) via adapters.
- Work items modeled as domain entities with queueing, SLAs, and escalation rules.
- SaaS connectors implemented as adapters in IntegrationHub.
- Webhooks supported for inbound and outbound events.

## Consequences

- Enables flexible workflow automation and integration with SaaS tools.
- Requires ongoing maintenance of BPMN models and connector adapters.

---

_This ADR is part of the Workflow Automation domain profile._
