
using CoreWorkflowService.Application.WorkflowCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace CoreWorkflowService.Api.Controllers;

/// <summary>
/// Manages workflow cases, including creation and retrieval by ID.
/// </summary>
[ApiController]
[Route("api/workflow-cases")]
public class WorkflowCasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkflowCasesController(IMediator mediator)
    {
        _mediator = mediator;
    }


    /// <summary>
    /// Creates a new workflow case.
    /// </summary>
    /// <param name="request">The workflow case details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workflow case.</returns>
    /// <response code="201">Returns the created workflow case.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkflowCaseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Creates a new workflow case.",
        Description = "Creates a new workflow case and returns the created object.",
        OperationId = "CreateWorkflowCase",
        Tags = new[] { "WorkflowCases" }
    )]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowCaseRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateWorkflowCaseCommand(request.Name), cancellationToken);
        if (result.IsFailure) return Problem(detail: result.Error.Message, statusCode: 400);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Gets a workflow case by ID.
    /// </summary>
    /// <param name="id">The workflow case ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workflow case details.</returns>
    /// <response code="200">Returns the workflow case.</response>
    /// <response code="404">If the workflow case is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowCaseDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [SwaggerOperation(
        Summary = "Gets a workflow case by ID.",
        Description = "Retrieves the workflow case details for the specified ID.",
        OperationId = "GetWorkflowCase",
        Tags = new[] { "WorkflowCases" }
    )]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWorkflowCaseQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound(result.Error.Message);
        return Ok(result.Value);
    }
    /// <summary>
    /// Example provider for WorkflowCaseDto response.
    /// </summary>
    public class WorkflowCaseDtoExampleProvider : IExamplesProvider<WorkflowCaseDto>
    {
        public WorkflowCaseDto GetExamples()
        {
            return new WorkflowCaseDto(
                Guid.NewGuid(),
                "Sample Workflow Case",
                "Active",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow
            );
        }
    }
}

public record CreateWorkflowCaseRequest(string Name);
