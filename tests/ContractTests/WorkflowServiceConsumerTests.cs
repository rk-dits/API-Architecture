using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Matchers;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace ContractTests.Consumers;

/// <summary>
/// Consumer contract tests for WorkflowService API
/// These tests define the expected contract from the consumer's perspective
/// </summary>
public class WorkflowServiceConsumerTests : IDisposable
{
    private readonly IPactBuilderV2 _pactBuilder;
    private readonly ITestOutputHelper _output;
    private readonly ILogger<WorkflowServiceConsumerTests> _logger;
    private readonly HttpClient _httpClient;

    public WorkflowServiceConsumerTests(ITestOutputHelper output)
    {
        _output = output;

        // Setup logging
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<WorkflowServiceConsumerTests>();

        // Create Pact builder
        var pactConfig = new PactConfig
        {
            PactDir = Path.Combine(Directory.GetCurrentDirectory(), "pacts"),
            Outputters = new[] { new XUnitOutput(_output) },
            LogLevel = PactLogLevel.Debug
        };

        _pactBuilder = Pact.V2("WorkflowClient", "WorkflowService", pactConfig);

        // Create HTTP client for mock server
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task GetWorkflowCase_WithValidId_ReturnsWorkflowCase()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResponse = new
        {
            id = workflowId,
            name = "Patient Onboarding Workflow",
            status = "active",
            priority = "high",
            createdAt = DateTime.UtcNow,
            steps = new[]
            {
                new { id = 1, name = "Collect Patient Information", status = "completed" },
                new { id = 2, name = "Verify Insurance", status = "in-progress" }
            }
        };

        _pactBuilder
            .UponReceiving("A request for a workflow case")
                .WithRequest(HttpMethod.Get, $"/api/v1/workflows/cases/{workflowId}")
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Accept", "application/json" },
                    { "Authorization", Match.Regex("Bearer .*", "Bearer valid-token") }
                })
            .WillRespondWith()
                .WithStatus(200)
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                })
                .WithJsonBody(Match.Type(expectedResponse));

        // Act & Assert
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid-token");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _httpClient.GetAsync($"/api/v1/workflows/cases/{workflowId}");

            // Verify response
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var content = await response.Content.ReadAsStringAsync();
            var workflowCase = JsonSerializer.Deserialize<JsonElement>(content);

            // Verify structure matches expected contract
            workflowCase.GetProperty("id").GetGuid().Should().Be(workflowId);
            workflowCase.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
            workflowCase.GetProperty("status").GetString().Should().BeOneOf("active", "completed", "cancelled");
            workflowCase.GetProperty("priority").GetString().Should().BeOneOf("low", "medium", "high");
            workflowCase.GetProperty("steps").GetArrayLength().Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task CreateWorkflowCase_WithValidData_ReturnsCreatedWorkflow()
    {
        // Arrange
        var createRequest = new
        {
            name = "New Patient Workflow",
            description = "Complete patient onboarding process",
            priority = "medium",
            templateId = Guid.NewGuid()
        };

        var expectedResponse = new
        {
            id = Match.Guid(),
            name = createRequest.name,
            description = createRequest.description,
            priority = createRequest.priority,
            status = "draft",
            createdAt = Match.ISO8601DateTime(),
            templateId = createRequest.templateId
        };

        _pactBuilder
            .UponReceiving("A request to create a workflow case")
                .WithRequest(HttpMethod.Post, "/api/v1/workflows/cases")
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", Match.Regex("Bearer .*", "Bearer valid-token") }
                })
                .WithJsonBody(createRequest)
            .WillRespondWith()
                .WithStatus(201)
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Location", Match.Regex("http.*", "http://localhost/api/v1/workflows/cases/123") }
                })
                .WithJsonBody(expectedResponse);

        // Act & Assert
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid-token");

            var requestJson = JsonSerializer.Serialize(createRequest);
            var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/workflows/cases", requestContent);

            // Verify response
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var content = await response.Content.ReadAsStringAsync();
            var workflowCase = JsonSerializer.Deserialize<JsonElement>(content);

            // Verify response structure
            workflowCase.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
            workflowCase.GetProperty("name").GetString().Should().Be(createRequest.name);
            workflowCase.GetProperty("priority").GetString().Should().Be(createRequest.priority);
            workflowCase.GetProperty("status").GetString().Should().Be("draft");
        });
    }

    [Fact]
    public async Task GetWorkflowCase_WithInvalidId_Returns404()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        _pactBuilder
            .UponReceiving("A request for a non-existent workflow case")
                .WithRequest(HttpMethod.Get, $"/api/v1/workflows/cases/{invalidId}")
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Accept", "application/json" },
                    { "Authorization", Match.Regex("Bearer .*", "Bearer valid-token") }
                })
            .WillRespondWith()
                .WithStatus(404)
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Content-Type", "application/problem+json" }
                })
                .WithJsonBody(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    title = "Not Found",
                    status = 404,
                    detail = Match.Type("Workflow case not found"),
                    instance = Match.Regex("/api/v1/workflows/cases/.*", $"/api/v1/workflows/cases/{invalidId}"),
                    traceId = Match.Type("trace-123")
                });

        // Act & Assert
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid-token");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _httpClient.GetAsync($"/api/v1/workflows/cases/{invalidId}");

            // Verify response
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

            // Verify error response structure follows RFC 7807
            problemDetails.GetProperty("type").GetString().Should().Contain("rfc7231");
            problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
            problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        });
    }

    [Fact]
    public async Task CreateWorkflowCase_WithInvalidData_Returns400()
    {
        // Arrange
        var invalidRequest = new
        {
            name = "", // Invalid: empty name
            priority = "invalid-priority", // Invalid: not in allowed values
            // Missing required fields
        };

        _pactBuilder
            .UponReceiving("A request to create workflow with invalid data")
                .WithRequest(HttpMethod.Post, "/api/v1/workflows/cases")
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", Match.Regex("Bearer .*", "Bearer valid-token") }
                })
                .WithJsonBody(invalidRequest)
            .WillRespondWith()
                .WithStatus(400)
                .WithHeaders(new Dictionary<string, object>
                {
                    { "Content-Type", "application/problem+json" }
                })
                .WithJsonBody(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Bad Request",
                    status = 400,
                    detail = Match.Type("Validation failed"),
                    instance = "/api/v1/workflows/cases",
                    traceId = Match.Type("trace-123"),
                    errors = Match.Type(new Dictionary<string, string[]>
                    {
                        { "name", new[] { "Name is required" } },
                        { "priority", new[] { "Priority must be low, medium, or high" } }
                    })
                });

        // Act & Assert
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            _httpClient.BaseAddress = ctx.MockServerUri;
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer valid-token");

            var requestJson = JsonSerializer.Serialize(invalidRequest);
            var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/workflows/cases", requestContent);

            // Verify response
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

            // Verify validation error structure
            problemDetails.GetProperty("status").GetInt32().Should().Be(400);
            problemDetails.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Object);
        });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

/// <summary>
/// Custom output writer for Pact to integrate with xUnit test output
/// </summary>
public class XUnitOutput : IOutput
{
    private readonly ITestOutputHelper _output;

    public XUnitOutput(ITestOutputHelper output)
    {
        _output = output;
    }

    public void WriteLine(string line)
    {
        _output.WriteLine(line);
    }
}