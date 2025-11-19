using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Persistence.Abstractions;
using CoreWorkflowService.Domain.Entities;
using MediatR;
using BuildingBlocks.Infrastructure.Tracing;
using System.Diagnostics;
using BuildingBlocks.Infrastructure.Compliance;

namespace CoreWorkflowService.Application.WorkflowCases;

public record CreateWorkflowCaseCommand(string Name) : IRequest<Result<WorkflowCaseDto>>;

public class CreateWorkflowCaseHandler : IRequestHandler<CreateWorkflowCaseCommand, Result<WorkflowCaseDto>>
{
    private readonly IRepository<WorkflowCase> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;
    private readonly IRedactionService _redactionService;

    public CreateWorkflowCaseHandler(IRepository<WorkflowCase> repository, IUnitOfWork unitOfWork, IAuditLogger auditLogger, IRedactionService redactionService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _redactionService = redactionService;
    }

    public async Task<Result<WorkflowCaseDto>> Handle(CreateWorkflowCaseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<WorkflowCaseDto>(Error.Validation("Name is required"));

        using var activity = PlatformActivity.Source.StartActivity("CreateWorkflowCaseHandler.Handle", ActivityKind.Internal);
        activity?.AddTag("workflow.name", request.Name);
        var entity = WorkflowCase.Create(request.Name);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        activity?.AddTag("workflow.id", entity.Id);
        activity?.SetStatus(ActivityStatusCode.Ok);
        var redacted = _redactionService.Redact(entity);
        _auditLogger.EntityCreated("WorkflowCase", redacted, new Dictionary<string, object?>
        {
            ["CorrelationId"] = activity?.Id,
            ["Timestamp"] = DateTime.UtcNow
        });
        return Result.Success(new WorkflowCaseDto(entity.Id, entity.Name, entity.Status.ToString(), entity.CreatedAt, entity.UpdatedAt));
    }
}

public record WorkflowCaseDto(Guid Id, string Name, string Status, DateTime CreatedAt, DateTime UpdatedAt);
