# Architecture Diagram (Acme Platform API)

```mermaid
graph TD
    subgraph API Gateway
        AG[ApiGateway]
    end
    subgraph Services
        IHA[IntegrationHub.Api]
        CWA[CoreWorkflowService.Api]
    end
    subgraph BuildingBlocks
        BBCommon[Common]
        BBInfra[Infrastructure]
        BBMsg[Messaging]
        BBPersist[Persistence]
    end
    AG -->|REST/YARP| IHA
    AG -->|REST/YARP| CWA
    IHA --> BBCommon
    IHA --> BBInfra
    IHA --> BBMsg
    IHA --> BBPersist
    CWA --> BBCommon
    CWA --> BBInfra
    CWA --> BBMsg
    CWA --> BBPersist
    AG --> BBCommon
    AG --> BBInfra
    AG --> BBMsg
    AG --> BBPersist
```

**Legend:**

- `ApiGateway`: Entry point, routing, security, and aggregation
- `IntegrationHub.Api` / `CoreWorkflowService.Api`: Bounded context services
- `BuildingBlocks`: Shared libraries for common, infrastructure, messaging, and persistence concerns

---

_This diagram is a high-level overview. For detailed flows, see sequence diagrams._
