# Contract Testing with Pact.NET

This directory contains contract tests for the Acme Platform using Pact.NET framework. Contract testing ensures that service integrations work correctly by testing the contracts between consumers and providers.

## Overview

Contract testing validates the integration between services by:

- **Consumer Tests**: Define expected contracts from the consumer's perspective
- **Provider Tests**: Verify that providers implement the expected contracts
- **Pact Files**: JSON files containing the contract specifications
- **Pact Broker**: Centralized storage and management of contracts

## Structure

```
ContractTests/
├── ContractTests.csproj          # Test project with Pact.NET dependencies
├── pact-config.json              # Pact configuration
├── WorkflowServiceConsumerTests.cs   # Consumer contract tests
├── WorkflowServiceProviderTests.cs   # Provider contract tests
├── pacts/                        # Generated pact files
└── logs/                         # Test execution logs
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker (for running services during provider verification)
- Pact Broker (optional, for CI/CD integration)

### Running Tests

#### 1. Consumer Tests (Generate Pacts)

```bash
# Run consumer tests to generate pact files
dotnet test --filter "WorkflowServiceConsumerTests"

# Generated pact files will be in ./pacts/ directory
ls ./pacts/
# WorkflowClient-WorkflowService.json
# WorkflowClient-IntegrationHub.json
```

#### 2. Provider Tests (Verify Contracts)

```bash
# Start the service under test
cd ../../src/BuildingBlocks/ApiGateway
dotnet run

# In another terminal, run provider verification tests
cd tests/ContractTests
dotnet test --filter "WorkflowServiceProviderTests"
```

### CI/CD Integration

#### GitHub Actions Example:

```yaml
name: Contract Tests

on: [push, pull_request]

jobs:
  consumer-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "8.0.x"

      - name: Run Consumer Tests
        run: dotnet test tests/ContractTests --filter "ConsumerTests"

      - name: Publish Pacts
        run: |
          dotnet tool install -g pact-net-cli
          pact-broker publish ./tests/ContractTests/pacts \
            --consumer-app-version ${{ github.sha }} \
            --branch ${{ github.ref_name }}

  provider-tests:
    needs: consumer-tests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3

      - name: Start Infrastructure
        run: docker-compose up -d

      - name: Run Provider Tests
        run: dotnet test tests/ContractTests --filter "ProviderTests"
        env:
          PACT_BROKER_BASE_URL: ${{ secrets.PACT_BROKER_URL }}
          PACT_BROKER_TOKEN: ${{ secrets.PACT_BROKER_TOKEN }}
```

## Contract Examples

### Consumer Test Example:

```csharp
[Fact]
public async Task GetWorkflowCase_WithValidId_ReturnsWorkflowCase()
{
    var workflowId = Guid.NewGuid();

    _pactBuilder
        .UponReceiving("A request for a workflow case")
            .WithRequest(HttpMethod.Get, $"/api/v1/workflows/cases/{workflowId}")
            .WithHeaders(new Dictionary<string, object>
            {
                { "Authorization", "Bearer valid-token" }
            })
        .WillRespondWith()
            .WithStatus(200)
            .WithJsonBody(new {
                id = Match.Guid(),
                name = Match.Type("Patient Onboarding"),
                status = Match.Regex("active|completed|cancelled", "active")
            });

    await _pactBuilder.VerifyAsync(async ctx =>
    {
        // Make actual HTTP request and verify response
        var response = await _httpClient.GetAsync($"/api/v1/workflows/cases/{workflowId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    });
}
```

### Provider State Management:

```csharp
public async Task SetupProviderState(string providerState, IDictionary<string, object> parameters)
{
    return providerState switch
    {
        "workflow case exists" => SetupExistingWorkflowCase(parameters),
        "user has valid authentication" => SetupValidAuth(parameters),
        _ => Task.CompletedTask
    };
}
```

## Testing Scenarios

### 1. WorkflowService Contracts

- **Get Workflow Case**: Valid ID returns workflow, invalid ID returns 404
- **Create Workflow Case**: Valid data creates workflow, invalid data returns validation errors
- **Update Workflow Case**: Authorized updates succeed, unauthorized fail
- **Delete Workflow Case**: Existing workflows can be deleted

### 2. IntegrationHub Contracts

- **Start Operation**: Valid operation request returns operation ID
- **Get Operation Status**: Returns current status and partial results
- **Provider Webhooks**: Accepts and validates webhook payloads
- **Real-time Updates**: WebSocket/SSE contract for live updates

### 3. Error Handling Contracts

- **Authentication Errors**: 401 for missing/invalid tokens
- **Authorization Errors**: 403 for insufficient permissions
- **Validation Errors**: 400 with RFC 7807 problem details
- **Not Found Errors**: 404 with consistent error structure
- **Server Errors**: 500 with correlation IDs for debugging

## Matchers and Validation

### Pact Matchers:

```csharp
// Type matching - ensures field is present and of correct type
Match.Type("example string")
Match.Type(123)
Match.Type(true)

// Regex matching - validates format
Match.Regex(@"^\d{4}-\d{2}-\d{2}$", "2024-01-15")
Match.Regex("Bearer .*", "Bearer token-123")

// Specific value matching
Match.Equality("active")

// Array matching
Match.MinType(1) // At least 1 item in array
Match.MaxType(10) // At most 10 items in array

// Date/time matching
Match.ISO8601DateTime()
Match.ISO8601Date()
Match.Timestamp()

// Numeric matching
Match.Integer()
Match.Decimal()
Match.Number()

// UUID matching
Match.Guid()
```

### Response Validation:

```csharp
// Verify response structure
var content = await response.Content.ReadAsStringAsync();
var json = JsonSerializer.Deserialize<JsonElement>(content);

json.GetProperty("id").GetGuid().Should().NotBeEmpty();
json.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
json.GetProperty("status").GetString().Should().BeOneOf("active", "completed", "cancelled");
```

## Configuration

### Pact Configuration (pact-config.json):

```json
{
  "pact": {
    "pactBroker": {
      "url": "https://pact-broker.acme.com",
      "token": "${PACT_BROKER_TOKEN}"
    },
    "provider": {
      "name": "WorkflowService",
      "version": "${BUILD_NUMBER}",
      "tags": ["${ENVIRONMENT}"]
    }
  },
  "verification": {
    "timeout": 30,
    "stateChangeUrl": "http://localhost:5000/pact/provider-states",
    "publishResults": true
  }
}
```

### Environment Variables:

- `PACT_BROKER_BASE_URL`: Pact broker URL
- `PACT_BROKER_TOKEN`: Authentication token for pact broker
- `BUILD_NUMBER`: Version number for contract versioning
- `GIT_BRANCH`: Git branch for contract organization
- `ENVIRONMENT`: Environment tag (dev/staging/prod)

## Best Practices

### 1. Consumer-Driven Contracts

- Consumers define what they need, not what providers offer
- Focus on actual usage scenarios, not all possible endpoints
- Keep contracts minimal and focused on business needs

### 2. Provider States

- Use provider states to set up specific test scenarios
- Keep state setup isolated and repeatable
- Clean up state after each test verification

### 3. Versioning Strategy

- Use semantic versioning for contract versions
- Tag contracts by environment and branch
- Maintain backward compatibility when possible

### 4. Test Data Management

- Use predictable test data in provider states
- Avoid dependencies on external systems during verification
- Mock external integrations for consistent test results

### 5. Error Scenarios

- Test both success and failure scenarios
- Verify error response formats match RFC standards
- Include authentication and authorization failures

## Troubleshooting

### Common Issues:

#### 1. Pact File Not Found

```
Error: Pact file not found: ./pacts/WorkflowClient-WorkflowService.json
Solution: Run consumer tests first to generate pact files
```

#### 2. Provider State Setup Fails

```
Error: Provider state 'workflow case exists' not found
Solution: Implement the provider state in TestProviderStateService
```

#### 3. Contract Verification Fails

```
Error: Expected status 200 but was 500
Solution: Check provider implementation and test data setup
```

#### 4. Authentication Issues

```
Error: 401 Unauthorized during contract verification
Solution: Configure test authentication or disable auth for contract tests
```

### Debug Commands:

```bash
# View generated pact files
cat ./pacts/WorkflowClient-WorkflowService.json | jq '.'

# Check pact broker contracts
curl -H "Authorization: Bearer $PACT_BROKER_TOKEN" \
  https://pact-broker.acme.com/pacts/provider/WorkflowService/consumer/WorkflowClient/latest

# Validate pact file format
pact-broker validate ./pacts/WorkflowClient-WorkflowService.json

# Test provider states endpoint
curl -X POST http://localhost:5000/pact/provider-states \
  -H "Content-Type: application/json" \
  -d '{"state": "workflow case exists", "params": {"id": "123"}}'
```

## Integration with Testing Pipeline

### 1. Pre-commit Hooks

- Run consumer tests to validate contract changes
- Check for breaking changes in contracts
- Lint and validate pact files

### 2. Pull Request Validation

- Verify contracts haven't been broken by changes
- Run both consumer and provider tests
- Check contract compatibility with other branches

### 3. Deployment Gates

- Verify all consumer contracts before deployment
- Ensure provider can handle existing consumer contracts
- Use can-i-deploy checks with pact broker

## Links and Resources

- [Pact.NET Documentation](https://docs.pact.io/implementation_guides/net/)
- [Contract Testing Best Practices](https://docs.pact.io/best_practices/consumer/)
- [Pact Broker Setup](https://docs.pact.io/pact_broker/)
- [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807)
