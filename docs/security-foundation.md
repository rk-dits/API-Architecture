# Security Foundation Implementation

This document outlines the security foundation implemented for the Acme Platform API.

## Overview

The security foundation provides:

- JWT/OAuth2 authentication
- Rate limiting protection
- Security headers middleware
- API Gateway security offloading
- Development token generation

## Components

### 1. Authentication & Authorization

#### JWT Authentication

- **Location**: `BuildingBlocks.Infrastructure.Security.AuthenticationServiceExtensions`
- **Configuration**: `Authentication:Jwt` section in appsettings.json
- **Features**:
  - JWT Bearer token validation
  - Configurable issuer/audience validation
  - Custom authentication failure handling
  - ProblemDetails integration

#### Authorization Policies

- `RequireAuthentication`: Basic authenticated user requirement
- `AdminOnly`: Admin role requirement
- `IntegrationAccess`: Integration scope requirement
- `WorkflowAccess`: Workflow scope requirement

### 2. Rate Limiting

#### Implementation

- **Location**: `BuildingBlocks.Infrastructure.Security.RateLimitingServiceExtensions`
- **Type**: In-memory sliding window rate limiter
- **Configuration**: `RateLimiting` section in appsettings.json

#### Policies

- **Global**: 100 requests/minute (default)
- **ApiPolicy**: 60 requests/minute (API endpoints)
- **IntegrationPolicy**: 30 requests/minute (integration endpoints)

#### Features

- User-based rate limiting (authenticated users)
- IP-based rate limiting (anonymous users)
- Client-ID based rate limiting
- Path-based policy selection
- ProblemDetails responses for rate limit exceeded

### 3. Security Headers

#### Implementation

- **Location**: `BuildingBlocks.Infrastructure.Middleware.SecurityHeadersMiddleware`
- **Headers Applied**:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `X-XSS-Protection: 1; mode=block`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: geolocation=(), microphone=(), camera=()`
  - `Content-Security-Policy: [restrictive policy]`
  - `Strict-Transport-Security: max-age=31536000; includeSubDomains; preload` (HTTPS only)

### 4. API Gateway Security

#### YARP Configuration

- **Routes**: Authenticated by default
- **Rate Limiting**: Applied per route policy
- **Health Checks**: Public access (no authentication)
- **Development Endpoints**: Development environment only

#### Gateway Features

- Authentication offloading
- Rate limiting before routing
- Security headers on all responses
- CORS policy configuration
- Development token generation

## Configuration

### JWT Configuration (Development)

```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "https://localhost:5000",
      "Audience": "acme-platform-dev",
      "SecretKey": "dev-secret-key-256-bits-long...",
      "ValidateIssuer": false,
      "ValidateAudience": false
    }
  }
}
```

### Rate Limiting Configuration

```json
{
  "RateLimiting": {
    "Global": {
      "PermitLimit": 100,
      "WindowSizeMinutes": 1
    },
    "Api": {
      "PermitLimit": 60,
      "WindowSizeMinutes": 1
    },
    "Integration": {
      "PermitLimit": 30,
      "WindowSizeMinutes": 1
    }
  }
}
```

## Development Tools

### Token Generation

**Endpoint**: `POST /api/devtoken/generate` (Development only)

**Request**:

```json
{
  "userId": "dev-user",
  "email": "dev@acme.local",
  "roles": ["User", "Developer"],
  "scopes": ["integrations.access", "workflows.access"]
}
```

**Response**:

```json
{
  "token": "eyJ...",
  "tokenType": "Bearer",
  "expiresInMinutes": 60,
  "userId": "dev-user",
  "email": "dev@acme.local",
  "roles": ["User", "Developer"],
  "scopes": ["integrations.access", "workflows.access"],
  "instructions": [
    "Use this token in the Authorization header as: Bearer {token}",
    "This token is valid for 60 minutes",
    "This endpoint is only available in development environment"
  ]
}
```

### Token Validation

**Endpoint**: `POST /api/devtoken/validate` (Development only)

**Request**:

```json
{
  "token": "eyJ..."
}
```

## Usage

### 1. Generate Development Token

```bash
curl -X POST https://localhost:5000/api/devtoken/generate \
  -H "Content-Type: application/json" \
  -d '{}'
```

### 2. Use Token for API Calls

```bash
curl -X GET https://localhost:5000/api/v1/integrations/operations \
  -H "Authorization: Bearer eyJ..."
```

### 3. Check Rate Limiting

Make multiple requests quickly to see rate limiting in action:

```bash
for i in {1..70}; do curl -s https://localhost:5000/api/v1/integrations/operations; done
```

## Service Ports

- **API Gateway**: https://localhost:5000
- **IntegrationHub**: https://localhost:5001
- **CoreWorkflowService**: https://localhost:5002

## Security Considerations

### Production Deployment

- Replace development JWT secret keys
- Configure proper JWT issuer/audience validation
- Enable certificate-based authentication
- Configure rate limiting based on actual usage patterns
- Implement distributed rate limiting (Redis-based)
- Add WAF and DDoS protection
- Configure proper CORS policies
- Enable audit logging

### Compliance Features

- All endpoints require authentication by default
- Security headers applied to all responses
- Rate limiting prevents abuse
- Audit trail via structured logging (when implemented)
- Support for PHI redaction (when implemented)

## Next Steps

1. Implement distributed rate limiting using Redis
2. Add OpenTelemetry security event tracing
3. Implement audit logging with structured events
4. Add certificate-based authentication for production
5. Configure Azure AD / Entra ID integration
6. Implement API key authentication for service-to-service
7. Add security event monitoring and alerting
