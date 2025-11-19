using IntegrationHub.Application.Operations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Common.PubSub;
using System.Threading.Channels;
using Swashbuckle.AspNetCore.Annotations;

namespace IntegrationHub.Api.Controllers;

/// <summary>
/// Manages integration operations and provides real-time event streaming endpoints.
/// </summary>
/// <remarks>
/// Endpoints for creating, retrieving, and streaming operation events (SSE, NDJSON).
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class OperationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPubSubService _pubSub;
    public OperationsController(IMediator mediator, IPubSubService pubSub)
    {
        _mediator = mediator;
        _pubSub = pubSub;
    }

    /// <summary>
    /// Creates a new integration operation.
    /// </summary>
    /// <param name="request">The operation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created operation.</returns>
    /// <response code="200">Returns the created operation.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OperationDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Creates a new integration operation.",
        Description = "Creates a new operation and returns the created operation object.",
        OperationId = "CreateOperation",
        Tags = new[] { "Operations" }
    )]
    // Example provider is available for documentation, but SwaggerResponseExample attribute is not supported in v7+
    public async Task<ActionResult<OperationDto>> Create([FromBody] CreateOperationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateOperationCommand(request.Name), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    /// <summary>
    /// Gets an operation by ID.
    /// </summary>
    /// <param name="id">The operation ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The operation details.</returns>
    /// <response code="200">Returns the operation.</response>
    /// <response code="404">If the operation is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OperationDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [SwaggerOperation(
        Summary = "Gets an operation by ID.",
        Description = "Retrieves the operation details for the specified operation ID.",
        OperationId = "GetOperation",
        Tags = new[] { "Operations" }
    )]
    // Example provider is available for documentation, but SwaggerResponseExample attribute is not supported in v7+
    public async Task<ActionResult<OperationDto>> Get(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOperationQuery(id), ct);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.Error.Code == "NotFound") return NotFound(result.Error.Message);
        return Problem(result.Error.Message, statusCode: 400);
    }

    /// <summary>
    /// Streams operation events as Server-Sent Events (SSE).
    /// </summary>
    /// <param name="id">The operation ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>Content-Type: text/event-stream</remarks>
    [HttpGet("{id:guid}/events")]
    [Produces("text/event-stream")]
    [SwaggerOperation(
        Summary = "Streams operation events as Server-Sent Events (SSE).",
        Description = "Streams real-time operation events for the specified operation ID using SSE.",
        OperationId = "StreamOperationEventsSSE",
        Tags = new[] { "Operations" }
    )]
    public async Task GetEvents(Guid id, CancellationToken ct)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        var channel = Channel.CreateUnbounded<string>();
        await _pubSub.SubscribeAsync($"operation:{id}", msg => channel.Writer.TryWrite(msg));
        await foreach (var msg in channel.Reader.ReadAllAsync(ct))
        {
            if (ct.IsCancellationRequested) break;
            await Response.WriteAsync($"data: {msg}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
    /// <summary>
    /// Example provider for OperationDto response.
    /// </summary>
    public class OperationDtoExampleProvider : Swashbuckle.AspNetCore.Filters.IExamplesProvider<OperationDto>
    {
        public OperationDto GetExamples()
        {
            return new OperationDto(
                Guid.NewGuid(),
                "Sample Operation",
                "Completed",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow
            );
        }
    }

    /// <summary>
    /// Streams operation events as NDJSON.
    /// </summary>
    /// <param name="id">The operation ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>Content-Type: application/x-ndjson</remarks>
    [HttpGet("{id:guid}/stream")]
    [Produces("application/x-ndjson")]
    [SwaggerOperation(OperationId = "StreamOperationEventsNDJSON", Tags = new[] { "Operations" })]
    public async Task GetEventsNdjson(Guid id, CancellationToken ct)
    {
        Response.Headers["Content-Type"] = "application/x-ndjson";
        var channel = Channel.CreateUnbounded<string>();
        await _pubSub.SubscribeAsync($"operation:{id}", msg => channel.Writer.TryWrite(msg));
        await foreach (var msg in channel.Reader.ReadAllAsync(ct))
        {
            if (ct.IsCancellationRequested) break;
            await Response.WriteAsync(msg + "\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
/// <summary>
/// Request payload for creating an operation.
/// </summary>
/// <example>{ "name": "Sample Operation" }</example>
public record CreateOperationRequest(string Name);