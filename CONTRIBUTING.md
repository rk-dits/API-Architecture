# Contributing to Acme Platform

Welcome to the Acme Platform! This guide will help you get up and running as a contributor to our microservices platform.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Adding a New Module](#adding-a-new-module)
- [10-Minute First Endpoint](#10-minute-first-endpoint)
- [Code Standards](#code-standards)
- [Testing Requirements](#testing-requirements)
- [Security Guidelines](#security-guidelines)
- [Documentation Standards](#documentation-standards)

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Docker Desktop
- PowerShell 7+ (for scripts)
- Git
- VS Code or Visual Studio 2022

### First-Time Setup

```powershell
# Clone the repository
git clone <repository-url>
cd API_Ready_Architecture

# Restore dependencies
dotnet restore

# Start local infrastructure
docker-compose up -d

# Build the solution
dotnet build

# Run tests
dotnet test

# Start the API Gateway
cd src/BuildingBlocks/ApiGateway
dotnet run
```

## Development Setup

### Local Infrastructure

Our `docker-compose.yml` provides:

- **PostgreSQL**: Primary database (port 5432)
- **Redis**: Caching and session storage (port 6379)
- **RabbitMQ**: Message broker (port 5672, management on 15672)
- **Jaeger**: Distributed tracing (port 16686)
- **Seq**: Structured logging (port 5341)

### Environment Configuration

Each service has environment-specific configurations:

- `appsettings.Development.json`: Local development
- `appsettings.Staging.json`: Staging environment
- `appsettings.Production.json`: Production environment
- `appsettings.Compliance.json`: HIPAA compliance overrides

## Contributing Guidelines

### Branching Strategy

- `main`: Production-ready code
- `develop`: Integration branch
- `feature/TICKET-123-description`: Feature branches
- `hotfix/critical-fix`: Production hotfixes

### Pull Request Process

1. Create a feature branch from `develop`
2. Implement your changes following our [Code Standards](#code-standards)
3. Add/update tests (minimum 80% coverage)
4. Update documentation
5. Run the full test suite locally
6. Submit PR with clear description and linked issues
7. Ensure all checks pass (CI, security scans, tests)
8. Request review from code owners

### Commit Message Format

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

feat(auth): add OAuth2 scope validation
fix(api): resolve null reference in workflow endpoint
docs(readme): update contribution guidelines
test(integration): add SignalR hub tests
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

## Adding a New Module

### Service Structure

Each service follows Clean Architecture with these layers:

```
/Services/YourService
  /YourService.Api          # Controllers, SignalR hubs, startup
  /YourService.Application  # Commands, queries, handlers (MediatR)
  /YourService.Domain      # Entities, value objects, domain events
  /YourService.Infrastructure # EF DbContext, repositories, external clients
  /YourService.Contracts   # DTOs, contracts for external consumption
```

### Step-by-Step Module Creation

1. **Create the service projects**:

```powershell
# From solution root
dotnet new webapi -n YourService.Api -o src/Services/YourService/YourService.Api
dotnet new classlib -n YourService.Application -o src/Services/YourService/YourService.Application
dotnet new classlib -n YourService.Domain -o src/Services/YourService/YourService.Domain
dotnet new classlib -n YourService.Infrastructure -o src/Services/YourService/YourService.Infrastructure
dotnet new classlib -n YourService.Contracts -o src/Services/YourService/YourService.Contracts

# Add to solution
dotnet sln add src/Services/YourService/YourService.Api
dotnet sln add src/Services/YourService/YourService.Application
dotnet sln add src/Services/YourService/YourService.Domain
dotnet sln add src/Services/YourService/YourService.Infrastructure
dotnet sln add src/Services/YourService/YourService.Contracts
```

2. **Add project references**:

```powershell
# Api -> Application, Infrastructure, Contracts
dotnet add src/Services/YourService/YourService.Api reference src/Services/YourService/YourService.Application
dotnet add src/Services/YourService/YourService.Api reference src/Services/YourService/YourService.Infrastructure
dotnet add src/Services/YourService/YourService.Api reference src/Services/YourService/YourService.Contracts

# Application -> Domain, Contracts
dotnet add src/Services/YourService/YourService.Application reference src/Services/YourService/YourService.Domain
dotnet add src/Services/YourService/YourService.Application reference src/Services/YourService/YourService.Contracts

# Infrastructure -> Application, Domain
dotnet add src/Services/YourService/YourService.Infrastructure reference src/Services/YourService/YourService.Application
dotnet add src/Services/YourService/YourService.Infrastructure reference src/Services/YourService/YourService.Domain

# Add BuildingBlocks references
dotnet add src/Services/YourService/YourService.Api reference src/BuildingBlocks/Common
dotnet add src/Services/YourService/YourService.Api reference src/BuildingBlocks/Infrastructure
dotnet add src/Services/YourService/YourService.Infrastructure reference src/BuildingBlocks/Persistence
dotnet add src/Services/YourService/YourService.Infrastructure reference src/BuildingBlocks/Messaging
```

3. **Add required NuGet packages** (see existing services for reference)

4. **Update API Gateway routing** in `src/BuildingBlocks/ApiGateway/appsettings.json`

5. **Create test projects**:

```powershell
dotnet new xunit -n YourService.UnitTests -o tests/YourService.UnitTests
dotnet new xunit -n YourService.IntegrationTests -o tests/YourService.IntegrationTests
dotnet sln add tests/YourService.UnitTests
dotnet sln add tests/YourService.IntegrationTests
```

## 10-Minute First Endpoint

Follow this guide to create your first endpoint in under 10 minutes:

### 1. Domain Model (2 minutes)

```csharp
// YourService.Domain/Entities/YourEntity.cs
public class YourEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private YourEntity() { } // EF Constructor

    public YourEntity(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        CreatedAt = DateTime.UtcNow;
    }
}
```

### 2. Command/Query (3 minutes)

```csharp
// YourService.Application/Commands/CreateYourEntityCommand.cs
public record CreateYourEntityCommand(string Name) : IRequest<Result<Guid>>;

public class CreateYourEntityCommandHandler : IRequestHandler<CreateYourEntityCommand, Result<Guid>>
{
    private readonly IYourEntityRepository _repository;

    public CreateYourEntityCommandHandler(IYourEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateYourEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = new YourEntity(request.Name);
        await _repository.AddAsync(entity, cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}
```

### 3. Repository Interface (1 minute)

```csharp
// YourService.Domain/Repositories/IYourEntityRepository.cs
public interface IYourEntityRepository
{
    Task<YourEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(YourEntity entity, CancellationToken cancellationToken = default);
}
```

### 4. Controller (3 minutes)

```csharp
// YourService.Api/Controllers/YourEntitiesController.cs
[ApiController]
[Route("api/v1/[controller]")]
public class YourEntitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public YourEntitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="command">Entity creation details</param>
    /// <returns>The ID of the created entity</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateYourEntityCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.ToProblemDetails());
        }

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        // Implement GetByIdQuery...
        return Ok();
    }
}
```

### 5. Wire up DI (1 minute)

Update your service's `Program.cs` or DI configuration to register the new handler and repository.

## Code Standards

### Naming Conventions

- **Projects**: `{Service}.{Layer}` (e.g., `CoreWorkflowService.Api`)
- **Namespaces**: Mirror folder structure
- **Entities**: Singular nouns (`WorkflowCase`, not `WorkflowCases`)
- **Collections**: Plural nouns (`var cases = new List<WorkflowCase>()`)
- **Async methods**: `Async` suffix (`GetByIdAsync`)
- **Interfaces**: `I` prefix (`IWorkflowRepository`)
- **DTOs**: `{Verb}{Resource}Request/Response` (`CreateWorkflowRequest`)

### C# Guidelines

- Use `nullable enable` in all projects
- Prefer `record` types for DTOs and value objects
- Use `required` members where appropriate
- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use implicit `using` statements (configured in `Directory.Build.props`)

### Architecture Rules

- **Domain**: No dependencies on infrastructure concerns
- **Application**: Can depend on Domain, not Infrastructure
- **Infrastructure**: Can depend on Application and Domain
- **Api**: Orchestrates, doesn't contain business logic
- **Clean separation**: Use interfaces and dependency injection

## Testing Requirements

### Test Categories

- **Unit Tests**: Fast, isolated, no external dependencies
- **Integration Tests**: Database, message broker, external APIs
- **Contract Tests**: API contracts using Pact
- **E2E Tests**: Full system behavior

### Coverage Requirements

- **Minimum**: 80% line coverage
- **Critical paths**: 95% coverage (authentication, payment, PHI handling)
- **Domain logic**: 100% coverage encouraged

### Test Structure

```csharp
[Fact]
public async Task CreateWorkflow_WithValidData_ReturnsSuccessResult()
{
    // Arrange
    var command = new CreateWorkflowCommand("Test Workflow");
    var handler = new CreateWorkflowCommandHandler(_mockRepository.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
}
```

### Integration Test Setup

Use Testcontainers for external dependencies:

```csharp
public class WorkflowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly PostgreSqlContainer _dbContainer;

    public WorkflowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _dbContainer = new PostgreSqlBuilder().Build();
    }
}
```

## Security Guidelines

### Input Validation

- Use FluentValidation for all commands/queries
- Validate at the API boundary
- Sanitize user inputs
- Use allow-lists for enums and constants

### Authentication & Authorization

- All endpoints require authentication unless explicitly marked `[AllowAnonymous]`
- Use scope-based authorization: `[Authorize("workflow:read")]`
- Implement least privilege principle
- Log all authentication/authorization events

### HIPAA Compliance

- Never log PHI (Personal Health Information)
- Use field-level encryption for sensitive data
- Implement audit trails for PHI access
- Follow data minimization principles

### Secrets Management

- Use Azure Key Vault for production secrets
- Never commit secrets to source control
- Use `dotnet user-secrets` for development
- Rotate secrets regularly

## Documentation Standards

### Code Documentation

- XML documentation comments for all public APIs
- Include examples in XML comments
- Document complex business logic
- Keep comments up-to-date with code changes

### API Documentation

- Use OpenAPI/Swagger annotations
- Provide request/response examples
- Document error scenarios
- Include deprecation notices

### Architecture Documentation

- Create ADRs for significant decisions
- Update diagrams when architecture changes
- Maintain runbooks for operational procedures
- Document integration patterns

### Example XML Documentation

```csharp
/// <summary>
/// Creates a new workflow case with the specified parameters.
/// </summary>
/// <param name="request">The workflow creation request containing case details</param>
/// <param name="cancellationToken">Cancellation token for the operation</param>
/// <returns>
/// A <see cref="Result{T}"/> containing the created workflow case ID if successful,
/// or error details if the operation failed.
/// </returns>
/// <example>
/// <code>
/// var request = new CreateWorkflowRequest("Patient Onboarding", "high");
/// var result = await CreateWorkflowAsync(request, cancellationToken);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine($"Created workflow: {result.Value}");
/// }
/// </code>
/// </example>
/// <exception cref="ArgumentNullException">Thrown when request is null</exception>
/// <exception cref="ValidationException">Thrown when request data is invalid</exception>
public async Task<Result<Guid>> CreateWorkflowAsync(
    CreateWorkflowRequest request,
    CancellationToken cancellationToken = default)
```

## Getting Help

- **Documentation**: Check `/docs` folder for detailed guides
- **Architecture Questions**: Review ADRs in `/docs`
- **Code Examples**: Look at existing services for patterns
- **Issues**: Create GitHub issues with detailed reproduction steps
- **Security Concerns**: Follow our security policy for reporting

## License

This project is licensed under the MIT License - see the LICENSE file for details.
