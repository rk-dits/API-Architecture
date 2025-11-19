# Sample SaaS Connector Adapter: AcmeSaaS

This adapter enables IntegrationHub to connect to AcmeSaaS for workflow automation.

## Overview

- Implements OAuth2 authentication
- Maps workflow triggers/actions to AcmeSaaS API endpoints
- Handles pagination, retries, and error mapping

## Example (C# Stub)

```csharp
public class AcmeSaaSAdapter : IWorkflowConnector
{
    public Task ConnectAsync(string token)
    {
        // Authenticate with AcmeSaaS using OAuth2 token
        // ...
        return Task.CompletedTask;
    }

    public Task TriggerActionAsync(string action, object payload)
    {
        // Map action to AcmeSaaS API endpoint
        // ...
        return Task.CompletedTask;
    }
}
```

## Usage

- Register in IntegrationHub.Infrastructure/DependencyInjection
- Configure credentials in appsettings.json
- Reference in workflow definitions

---

_This is a sample stub for a SaaS workflow connector._
