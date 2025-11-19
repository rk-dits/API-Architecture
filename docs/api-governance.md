# API Governance Guidelines

This document establishes standards and best practices for API design, development, and lifecycle management across the Acme Platform.

## Table of Contents

- [API Design Principles](#api-design-principles)
- [REST API Standards](#rest-api-standards)
- [Naming Conventions](#naming-conventions)
- [Versioning Strategy](#versioning-strategy)
- [Deprecation Workflow](#deprecation-workflow)
- [Documentation Requirements](#documentation-requirements)
- [Quality Gates](#quality-gates)
- [Security Standards](#security-standards)
- [Monitoring and SLAs](#monitoring-and-slas)

## API Design Principles

### 1. API-First Design

- Design APIs before implementation
- Use OpenAPI specifications as contracts
- Validate designs with stakeholders before coding
- Generate client SDKs from specifications

### 2. Consumer-Centric

- Design for ease of use, not implementation convenience
- Provide clear, consistent interfaces
- Include comprehensive examples and documentation
- Support multiple content types where appropriate

### 3. Backward Compatibility

- Never break existing consumers without proper deprecation
- Use semantic versioning for breaking changes
- Provide migration guides for major versions
- Maintain backward compatibility for at least 18 months

### 4. Consistency

- Follow consistent patterns across all APIs
- Use standard HTTP status codes and methods
- Implement uniform error handling
- Apply consistent naming conventions

## REST API Standards

### HTTP Methods

Use HTTP methods according to their semantic meaning:

| Method  | Purpose                 | Idempotent | Safe |
| ------- | ----------------------- | ---------- | ---- |
| GET     | Retrieve resources      | ✅         | ✅   |
| POST    | Create resources        | ❌         | ❌   |
| PUT     | Replace entire resource | ✅         | ❌   |
| PATCH   | Partial update          | ❌         | ❌   |
| DELETE  | Remove resource         | ✅         | ❌   |
| HEAD    | Get headers only        | ✅         | ✅   |
| OPTIONS | Get allowed methods     | ✅         | ✅   |

### HTTP Status Codes

Use standard HTTP status codes consistently:

#### Success (2xx)

- `200 OK`: Successful GET, PUT, PATCH, or DELETE
- `201 Created`: Successful POST that creates a resource
- `202 Accepted`: Request accepted for async processing
- `204 No Content`: Successful request with no response body

#### Client Errors (4xx)

- `400 Bad Request`: Invalid request syntax or data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Authentication successful but insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict (e.g., duplicate creation)
- `422 Unprocessable Entity`: Valid syntax but semantic errors

#### Server Errors (5xx)

- `500 Internal Server Error`: Generic server error
- `502 Bad Gateway`: Invalid response from upstream server
- `503 Service Unavailable`: Service temporarily unavailable
- `504 Gateway Timeout`: Upstream server timeout

### Request/Response Patterns

#### Successful Responses

```json
// Single resource
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Workflow Case",
  "status": "active",
  "createdAt": "2024-01-15T10:00:00Z"
}

// Collection with metadata
{
  "data": [
    { "id": "1", "name": "Item 1" },
    { "id": "2", "name": "Item 2" }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalItems": 95
  }
}
```

#### Error Responses (RFC 7807)

```json
{
  "type": "https://api.acme.com/problems/validation-error",
  "title": "Validation Failed",
  "status": 400,
  "detail": "The request contains invalid data",
  "instance": "/api/v1/workflows/cases",
  "traceId": "00-abc123-def456-01",
  "errors": {
    "name": ["Name is required"],
    "priority": ["Priority must be low, medium, or high"]
  }
}
```

## Naming Conventions

### Resource Naming

- Use **plural nouns** for collections: `/api/v1/workflows`, `/api/v1/users`
- Use **lowercase** with **hyphens** for multi-word resources: `/workflow-cases`
- Be **descriptive** but **concise**: `/patients` not `/pat` or `/patientRecords`
- Avoid **verbs** in resource names: `/workflows` not `/createWorkflow`

### Nested Resources

```
/workflows/{workflowId}/cases              # Get cases for a workflow
/workflows/{workflowId}/cases/{caseId}     # Get specific case
/users/{userId}/permissions                # Get user permissions
```

Limit nesting to 2-3 levels maximum for readability.

### Query Parameters

- Use **camelCase**: `?sortBy=createdDate&includeInactive=false`
- Be **descriptive**: `?status=active` not `?s=a`
- Support common patterns:
  - **Pagination**: `page`, `pageSize`, `offset`, `limit`
  - **Sorting**: `sortBy`, `sortOrder` (asc/desc)
  - **Filtering**: `status`, `createdAfter`, `type`
  - **Field selection**: `fields=id,name,status`

### Property Naming

- Use **camelCase** in JSON: `firstName`, `createdAt`, `isActive`
- Use **consistent** date formats: ISO 8601 (`2024-01-15T10:00:00Z`)
- Use **meaningful** names: `phoneNumber` not `phone`
- Follow **boolean** conventions: `isActive`, `hasPermission`, `canEdit`

## Versioning Strategy

### Version Format

Use semantic versioning in the URL path: `/api/v1/`, `/api/v2/`

### Version Lifecycle

1. **Development**: Pre-release versions (v1.0.0-beta)
2. **Stable**: Production-ready versions (v1.0.0)
3. **Deprecated**: Marked for future removal (v1.0.0 + deprecation headers)
4. **Sunset**: End-of-life date announced
5. **Removed**: Version no longer available

### Breaking vs Non-Breaking Changes

#### Non-Breaking (Patch/Minor versions)

✅ Adding new optional fields  
✅ Adding new endpoints  
✅ Adding new optional query parameters  
✅ Adding new response fields  
✅ Relaxing validation rules

#### Breaking (Major versions)

❌ Removing fields or endpoints  
❌ Renaming fields  
❌ Changing field types  
❌ Making optional fields required  
❌ Changing URL structure  
❌ Stricter validation rules

### Version Headers

Include version information in responses:

```http
API-Version: 1.0
Supported-Versions: 1.0,2.0
Deprecation: Sun, 01 Jan 2025 00:00:00 GMT
Sunset: Sun, 01 Jul 2025 00:00:00 GMT
```

## Deprecation Workflow

### Deprecation Timeline

1. **Announcement**: 6 months before deprecation
2. **Deprecation Headers**: Added to responses
3. **Sunset Notice**: 3 months before removal
4. **Final Warning**: 1 month before removal
5. **Removal**: API version retired

### Communication Plan

- **Email notifications** to registered API consumers
- **Developer portal** announcements and banners
- **Release notes** and changelog updates
- **Slack/Teams** notifications for internal teams
- **Migration guides** and tooling support

### Deprecation Headers

```http
Deprecation: Sat, 01 Jan 2025 00:00:00 GMT
Sunset: Sat, 01 Jul 2025 00:00:00 GMT
Link: <https://api.acme.com/docs/migration/v2>; rel=\"successor-version\"
Warning: 299 - \"This API version is deprecated. Please migrate to v2. See migration guide at https://api.acme.com/docs/migration/v2\"
```

### Migration Support

- Provide **automated migration tools** where possible
- Offer **dual-write periods** for data migrations
- Create **compatibility layers** for common use cases
- Provide **dedicated support** during transition periods

## Documentation Requirements

### OpenAPI Specification

All APIs must include comprehensive OpenAPI documentation:

```yaml
paths:
  /api/v1/workflows:
    post:
      operationId: createWorkflow
      summary: Create a new workflow
      description: |
        Creates a new workflow with the specified configuration.
        The workflow will be in 'draft' status initially.
      tags: [Workflows]
      security:
        - BearerAuth: [workflow:create]
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/CreateWorkflowRequest"
            examples:
              patient-onboarding:
                summary: Patient onboarding workflow
                value:
                  name: "Patient Onboarding"
                  description: "Complete patient registration process"
                  priority: "high"
      responses:
        "201":
          description: Workflow created successfully
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/WorkflowResponse"
```

### Required Documentation Elements

- **Summary and description** for all endpoints
- **Request/response examples** for common use cases
- **Error scenarios** and troubleshooting guides
- **Authentication and authorization** requirements
- **Rate limiting** information
- **Pagination** behavior
- **Field validation** rules
- **Business logic** explanations

### Code Examples

Provide examples in multiple languages:

```csharp
// C#
var client = new WorkflowClient();
var request = new CreateWorkflowRequest
{
    Name = "Patient Onboarding",
    Priority = "high"
};
var workflow = await client.CreateWorkflowAsync(request);
```

```javascript
// JavaScript
const response = await fetch("/api/v1/workflows", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    name: "Patient Onboarding",
    priority: "high",
  }),
});
const workflow = await response.json();
```

## Quality Gates

### Pre-Development

- [ ] OpenAPI specification reviewed and approved
- [ ] Security review completed
- [ ] Performance requirements defined
- [ ] Consumer feedback incorporated

### Development

- [ ] Implementation matches OpenAPI specification
- [ ] Unit tests coverage ≥ 80%
- [ ] Integration tests for all endpoints
- [ ] Contract tests with Pact (for external APIs)
- [ ] Load tests for expected traffic

### Pre-Release

- [ ] Security scan (SAST/DAST) passed
- [ ] Documentation updated and reviewed
- [ ] Breaking change analysis completed
- [ ] Migration guide created (if needed)
- [ ] Monitoring and alerts configured

### Post-Release

- [ ] API metrics and SLAs monitored
- [ ] Consumer feedback collected
- [ ] Performance baseline established
- [ ] Error rates within acceptable limits

### Automated Checks

```yaml
# Example GitHub Actions quality gates
- name: OpenAPI Lint
  run: spectral lint openapi.yml

- name: Breaking Change Detection
  run: oasdiff breaking openapi-old.yml openapi-new.yml

- name: Contract Tests
  run: pact-broker can-i-deploy --pacticipant ApiService
```

## Security Standards

### Authentication

- **OAuth 2.0 + OIDC** for user authentication
- **Client credentials** for service-to-service
- **API keys** for simple integrations (with limitations)
- **mTLS** for high-security scenarios

### Authorization

```http
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...

# Scope-based authorization
GET /api/v1/workflows     # Requires: workflow:read
POST /api/v1/workflows    # Requires: workflow:create
PUT /api/v1/workflows/123 # Requires: workflow:update
```

### Security Headers

```http
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

### Input Validation

- **Validate all inputs** at API boundary
- Use **allow-lists** for enums and constrained values
- **Sanitize** user-provided content
- **Limit** request sizes and rate limits
- **Log** security events for monitoring

### PII/PHI Protection

- **Never log** sensitive data (PII/PHI)
- Use **field-level encryption** for sensitive fields
- Implement **data minimization** principles
- Provide **audit trails** for data access
- Support **right to erasure** (GDPR/HIPAA)

## Monitoring and SLAs

### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime (43 minutes downtime/month)
- **Latency**: P95 < 200ms, P99 < 500ms
- **Error Rate**: < 0.1% for 4xx/5xx responses
- **Throughput**: Handle expected peak load + 20% buffer

### Key Metrics

- **Request volume** and patterns
- **Response times** (P50, P95, P99)
- **Error rates** by endpoint and status code
- **Authentication failures** and security events
- **Downstream dependency** health

### Alerting Rules

```yaml
# Example Prometheus alerting rules
- alert: HighErrorRate
  expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.01
  for: 2m
  annotations:
    summary: "High error rate on {{ $labels.endpoint }}"

- alert: HighLatency
  expr: histogram_quantile(0.95, http_request_duration_seconds) > 0.5
  for: 5m
  annotations:
    summary: "High latency on {{ $labels.endpoint }}"
```

### Dashboards

Create Grafana dashboards for:

- **API Overview**: Request volume, latency, errors
- **Endpoint Details**: Per-endpoint metrics and trends
- **Security Events**: Authentication, authorization failures
- **Business Metrics**: Feature usage, conversion rates

### Log Structure

```json
{
  "timestamp": "2024-01-15T10:00:00Z",
  "level": "info",
  "traceId": "00-abc123-def456-01",
  "spanId": "def456",
  "userId": "user123",
  "endpoint": "/api/v1/workflows",
  "method": "POST",
  "statusCode": 201,
  "duration": 150,
  "userAgent": "MyApp/1.0"
}
```

## Governance Process

### API Review Board

- **Membership**: Architects, security, product owners
- **Meeting cadence**: Bi-weekly
- **Responsibilities**: Review designs, approve breaking changes, set standards

### Change Management

1. **RFC Process** for significant API changes
2. **Impact Assessment** for breaking changes
3. **Stakeholder Approval** before implementation
4. **Phased Rollout** for high-risk changes

### Compliance Monitoring

- **Automated scanning** for standard violations
- **Regular audits** of API quality and security
- **Consumer feedback** collection and analysis
- **Continuous improvement** based on metrics and feedback

## Tools and Resources

### Development Tools

- **OpenAPI Generator**: Generate client SDKs and documentation
- **Spectral**: Lint OpenAPI specifications for quality
- **Swagger UI**: Interactive API documentation
- **Postman/Insomnia**: API testing and development

### Quality Assurance

- **oasdiff**: Detect breaking changes in OpenAPI specs
- **Pact**: Contract testing between services
- **Newman**: Automated Postman collection testing
- **k6**: Load and performance testing

### Monitoring

- **Application Insights**: Request tracking and analytics
- **Prometheus + Grafana**: Metrics collection and visualization
- **Jaeger**: Distributed tracing
- **Seq/ELK**: Centralized logging

## References

- [REST API Guidelines (Microsoft)](https://github.com/Microsoft/api-guidelines)
- [OpenAPI Specification](https://swagger.io/specification/)
- [RFC 7807 - Problem Details](https://tools.ietf.org/html/rfc7807)
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/draft-ietf-oauth-security-topics)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
