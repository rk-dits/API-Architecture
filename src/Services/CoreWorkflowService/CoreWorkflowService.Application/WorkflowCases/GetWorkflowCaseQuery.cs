using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Persistence.Abstractions;
using CoreWorkflowService.Domain.Entities;
using MediatR;
using BuildingBlocks.Infrastructure.Tracing;
using System.Diagnostics;

namespace CoreWorkflowService.Application.WorkflowCases;

public record GetWorkflowCaseQuery(Guid Id) : IRequest<Result<WorkflowCaseDto>>;

public class GetWorkflowCaseHandler : IRequestHandler<GetWorkflowCaseQuery, Result<WorkflowCaseDto>>
{
    private readonly IRepository<WorkflowCase> _repository;

    public GetWorkflowCaseHandler(IRepository<WorkflowCase> repository)
    {
        _repository = repository;
    }

    public async Task<Result<WorkflowCaseDto>> Handle(GetWorkflowCaseQuery request, CancellationToken cancellationToken)
    {
        using var activity = PlatformActivity.Source.StartActivity("GetWorkflowCaseHandler.Handle", ActivityKind.Internal);
        activity?.AddTag("workflow.id", request.Id);
        var entity = await _repository.GetAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure<WorkflowCaseDto>(Error.NotFound("WorkflowCase", request.Id.ToString()));
        activity?.SetStatus(ActivityStatusCode.Ok);
        return Result.Success(new WorkflowCaseDto(entity.Id, entity.Name, entity.Status.ToString(), entity.CreatedAt, entity.UpdatedAt));
    }
}
