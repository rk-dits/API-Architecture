# Sequence Diagram: API Request Flow (Acme Platform)

```mermaid
sequenceDiagram
    participant Client
    participant AG as ApiGateway
    participant SVC as Service (IntegrationHub/CoreWorkflow)
    participant BB as BuildingBlocks (Common/Infra/Messaging/Persistence)

    Client->>AG: HTTP Request (REST)
    AG->>AG: Authenticate & Authorize (JWT, Policies)
    AG->>AG: Rate Limiting, Security Headers
    AG->>SVC: Forward Request (YARP/REST)
    SVC->>BB: Use BuildingBlocks (e.g., DB, Messaging)
    SVC-->>AG: Response
    AG-->>Client: HTTP Response
```

**Legend:**

- `ApiGateway`: Handles security, routing, and aggregation
- `Service`: Bounded context (IntegrationHub or CoreWorkflow)
- `BuildingBlocks`: Shared libraries for cross-cutting concerns

---

_This diagram shows a typical request flow from client to service and back, including security and infrastructure steps._
