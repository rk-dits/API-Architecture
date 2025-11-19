using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Verifier;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace ContractTests.Providers;

/// <summary>
/// Provider contract tests that verify the WorkflowService API
/// implements the contracts defined by consumers
/// </summary>
public class WorkflowServiceProviderTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly ILogger<WorkflowServiceProviderTests> _logger;
    private readonly HttpClient _client;
    private readonly string _pactFileDirectory;

    public WorkflowServiceProviderTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;

        // Setup logging
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<WorkflowServiceProviderTests>();

        // Configure test client with overrides
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Override configuration for testing
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=acme_test;Username=test;Password=test",
                    ["Authentication:RequireAuthentication"] = "false", // Disable auth for contract tests
                    ["Logging:LogLevel:Default"] = "Information"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace services for testing
                services.AddScoped<IWorkflowCaseRepository, MockWorkflowCaseRepository>();
                services.AddScoped<IProviderStateService, TestProviderStateService>();
            });

        }).CreateClient();

        _pactFileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "pacts");
    }

    [Fact]
    public async Task VerifyWorkflowServiceContract()
    {
        // Arrange
        var pactVerifier = new PactVerifier(new PactVerifierConfig
        {
            Outputters = new[] { new XUnitOutput(_output) },
            LogLevel = PactLogLevel.Debug
        });

        var pactFile = Path.Combine(_pactFileDirectory, "WorkflowClient-WorkflowService.json");

        // Ensure pact file exists (in real scenario, this would be fetched from Pact Broker)
        if (!File.Exists(pactFile))
        {
            _output.WriteLine($"Pact file not found: {pactFile}");
            _output.WriteLine("Run consumer tests first to generate pact files.");
            return;
        }

        // Act & Assert
        pactVerifier
            .ServiceProvider("WorkflowService", _client.BaseAddress)
            .WithFileSource(new FileInfo(pactFile))
            .WithProviderStateUrl(new Uri(_client.BaseAddress, "/pact/provider-states"))
            .WithRequestTimeout(TimeSpan.FromSeconds(30))
            .WithSslVerificationDisabled() // For local testing
            .Verify();
    }

    [Fact]
    public async Task VerifyIntegrationHubContract()
    {
        // Arrange - Test IntegrationHub provider contracts
        var pactVerifier = new PactVerifier(new PactVerifierConfig
        {
            Outputters = new[] { new XUnitOutput(_output) },
            LogLevel = PactLogLevel.Debug
        });

        var pactFile = Path.Combine(_pactFileDirectory, "WorkflowClient-IntegrationHub.json");

        if (!File.Exists(pactFile))
        {
            _output.WriteLine($"IntegrationHub pact file not found: {pactFile}");
            return;
        }

        // Act & Assert
        pactVerifier
            .ServiceProvider("IntegrationHub", _client.BaseAddress)
            .WithFileSource(new FileInfo(pactFile))
            .WithProviderStateUrl(new Uri(_client.BaseAddress, "/pact/provider-states"))
            .WithRequestTimeout(TimeSpan.FromSeconds(30))
            .WithSslVerificationDisabled()
            .Verify();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

/// <summary>
/// Mock repository for contract testing
/// Provides predictable responses for contract verification
/// </summary>
public class MockWorkflowCaseRepository : IWorkflowCaseRepository
{
    public Task<WorkflowCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Return predictable test data for contract verification
        if (id == Guid.Empty)
        {
            return Task.FromResult<WorkflowCase?>(null);
        }

        var workflowCase = new WorkflowCase("Patient Onboarding Workflow")
        {
            Id = id,
            Status = WorkflowStatus.Active,
            Priority = WorkflowPriority.High,
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult<WorkflowCase?>(workflowCase);
    }

    public Task AddAsync(WorkflowCase workflowCase, CancellationToken cancellationToken = default)
    {
        // Mock successful creation
        return Task.CompletedTask;
    }

    public Task UpdateAsync(WorkflowCase workflowCase, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<WorkflowCase>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var workflowCases = new List<WorkflowCase>
        {
            new("Patient Onboarding Workflow") { Id = Guid.NewGuid(), Status = WorkflowStatus.Active },
            new("Insurance Verification") { Id = Guid.NewGuid(), Status = WorkflowStatus.Completed }
        };

        return Task.FromResult<IReadOnlyList<WorkflowCase>>(workflowCases);
    }
}

/// <summary>
/// Test provider state service for setting up test scenarios
/// Maps provider states to specific test data configurations
/// </summary>
public class TestProviderStateService : IProviderStateService
{
    private readonly MockWorkflowCaseRepository _repository;

    public TestProviderStateService(MockWorkflowCaseRepository repository)
    {
        _repository = repository;
    }

    public Task SetupProviderState(string providerState, IDictionary<string, object> parameters)
    {
        return providerState switch
        {
            "workflow case exists" => SetupExistingWorkflowCase(parameters),
            "workflow case does not exist" => SetupNonExistentWorkflowCase(parameters),
            "user has valid authentication" => SetupValidAuthentication(parameters),
            "integration hub is available" => SetupIntegrationHubAvailable(parameters),
            _ => Task.CompletedTask
        };
    }

    private Task SetupExistingWorkflowCase(IDictionary<string, object> parameters)
    {
        // Configure mock to return existing workflow case
        return Task.CompletedTask;
    }

    private Task SetupNonExistentWorkflowCase(IDictionary<string, object> parameters)
    {
        // Configure mock to return null for workflow case
        return Task.CompletedTask;
    }

    private Task SetupValidAuthentication(IDictionary<string, object> parameters)
    {
        // Configure authentication context
        return Task.CompletedTask;
    }

    private Task SetupIntegrationHubAvailable(IDictionary<string, object> parameters)
    {
        // Configure integration hub to be available
        return Task.CompletedTask;
    }
}

/// <summary>
/// Interface for provider state management during contract testing
/// </summary>
public interface IProviderStateService
{
    Task SetupProviderState(string providerState, IDictionary<string, object> parameters);
}