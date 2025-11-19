# ADR-0001: Use YARP as API Gateway

## Status

- **Status**: Accepted
- **Date**: 2024-01-15
- **Deciders**: Solution Architecture Team, Platform Team
- **Technical Story**: Initial platform architecture decision

## Context and Problem Statement

We need to choose an API Gateway solution for our microservices platform that can handle authentication, authorization, rate limiting, routing, and load balancing. The solution must be .NET-native, cloud-ready, and maintainable for 20+ years without architectural rewrites.

## Decision Drivers

- **Performance**: Low latency, high throughput requirements
- **Maintainability**: Long-term support and .NET ecosystem alignment
- **Flexibility**: Extensible for custom middleware and policies
- **Cloud-native**: Kubernetes-ready with health checks and observability
- **Security**: Built-in support for OAuth2/OIDC, rate limiting, CORS
- **Cost**: Open-source with enterprise support options
- **Team expertise**: .NET developers can extend and maintain

## Considered Options

- **Option 1**: YARP (Yet Another Reverse Proxy)
- **Option 2**: Ocelot API Gateway
- **Option 3**: Kong Gateway
- **Option 4**: Azure Application Gateway + APIM
- **Option 5**: Envoy Proxy

## Decision Outcome

**Chosen option**: "YARP (Yet Another Reverse Proxy)", because it provides the best balance of performance, .NET integration, flexibility, and long-term maintainability for our platform requirements.

### Positive Consequences

- Native .NET 8 integration with ASP.NET Core pipeline
- Excellent performance and memory efficiency
- Extensible middleware model for custom policies
- Strong Microsoft backing and active development
- Easy to test, debug, and deploy with existing .NET tooling
- Built-in health checks, metrics, and observability integration
- Configuration-driven with hot reload capabilities

### Negative Consequences

- Newer project with smaller ecosystem compared to Kong/Envoy
- Less third-party tooling and plugins available
- Requires custom development for some advanced features
- Documentation still evolving

## Pros and Cons of the Options

### YARP (Yet Another Reverse Proxy)

Microsoft-developed reverse proxy built on ASP.NET Core

**Pros**

- Native .NET performance and integration
- Highly configurable and extensible
- Active development and Microsoft support
- Built-in load balancing, health checks, rate limiting
- Easy debugging and development experience
- Configuration hot reload
- Strong observability integration (OpenTelemetry)

**Cons**

- Newer project with evolving feature set
- Smaller community compared to alternatives
- Some advanced features require custom development

### Ocelot API Gateway

Popular .NET API Gateway framework

**Pros**

- Mature .NET ecosystem project
- Rich feature set out of the box
- Good documentation and community
- JWT authentication built-in

**Cons**

- Performance limitations under high load
- Less flexible middleware model
- Configuration complexity for advanced scenarios
- Maintenance concerns for long-term sustainability

### Kong Gateway

Enterprise-grade API Gateway

**Pros**

- Very mature with extensive plugin ecosystem
- Excellent performance and scalability
- Strong enterprise features
- Great documentation and community

**Cons**

- Not .NET native (Lua/OpenResty based)
- Complex deployment and configuration
- Licensing costs for enterprise features
- Different technology stack from our platform

### Azure Application Gateway + APIM

Azure-native gateway solution

**Pros**

- Fully managed Azure service
- Enterprise-grade features
- Integrated with Azure ecosystem
- Built-in WAF and security features

**Cons**

- Vendor lock-in to Azure
- Higher costs for high-volume scenarios
- Less flexibility for custom logic
- Slower development iteration cycle

### Envoy Proxy

CNCF graduated service mesh proxy

**Pros**

- Excellent performance and reliability
- Rich feature set and extensibility
- Strong community and industry adoption
- Service mesh integration ready

**Cons**

- C++ based, different from our .NET stack
- Complex configuration (xDS protocol)
- Steep learning curve
- Requires additional control plane

## Implementation Details

- Deploy YARP as a dedicated ApiGateway service in our platform
- Configure routes, clusters, and policies via appsettings.json
- Implement custom middleware for:
  - JWT token validation and scope checking
  - Rate limiting with Redis backend
  - Request/response logging and correlation
  - Circuit breaker patterns with Polly
- Use health checks for backend service discovery
- Integrate with OpenTelemetry for distributed tracing
- Configure CORS policies for web client support

### Configuration Structure

```json
{
  "ReverseProxy": {
    "Routes": {
      "workflow-route": {
        "ClusterId": "workflow-cluster",
        "Match": {
          "Path": "/api/workflows/{**catch-all}"
        },
        "Transforms": [{ "PathPattern": "/api/v1/workflows/{**catch-all}" }]
      }
    },
    "Clusters": {
      "workflow-cluster": {
        "Destinations": {
          "workflow-service": {
            "Address": "http://coreworkflowservice:8080/"
          }
        }
      }
    }
  }
}
```

## Links

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Platform Architecture Overview](architecture-diagram.md)

## Notes

This decision establishes YARP as our standard API Gateway. Future ADRs should document specific middleware implementations, security policies, and routing strategies as they are developed.
