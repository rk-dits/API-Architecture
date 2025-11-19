using BuildingBlocks.Persistence.Abstractions;
using IntegrationHub.Domain.Entities;
using MediatR;
using BuildingBlocks.Common.Abstractions; // Result & Error
using BuildingBlocks.Infrastructure.Tracing;
using System.Diagnostics;

using BuildingBlocks.Infrastructure.Compliance;
using IntegrationHub.Contracts.Events;

namespace IntegrationHub.Application.Operations;

/// <summary>
/// Command to create a new integration operation.
/// </summary>
public record CreateOperationCommand(string Name) : IRequest<Result<OperationDto>>;

/// <summary>
/// Handles <see cref="CreateOperationCommand"/> producing an <see cref="OperationDto"/>.
/// </summary>

public interface IOperationEventPublisher
{
    Task PublishCreatedAsync(Guid operationId, string name);
}

public class CreateOperationHandler : IRequestHandler<CreateOperationCommand, Result<OperationDto>>
{
    private readonly IRepository<Operation> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;
    private readonly IRedactionService _redactionService;
    private readonly IOperationEventPublisher _eventPublisher;

    public CreateOperationHandler(
        IRepository<Operation> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger auditLogger,
        IRedactionService redactionService,
        IOperationEventPublisher eventPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _redactionService = redactionService;
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc />
    public async Task<Result<OperationDto>> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<OperationDto>(Error.Validation("Name is required"));

        using var activity = PlatformActivity.Source.StartActivity("CreateOperationHandler.Handle", ActivityKind.Internal);
        activity?.AddTag("operation.name", request.Name);

        var op = Operation.Create(request.Name);
        await _repository.AddAsync(op, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        activity?.AddTag("operation.id", op.Id);
        activity?.SetStatus(ActivityStatusCode.Ok);

        // Audit (redacted) after persistence
        var redacted = _redactionService.Redact(op);
        _auditLogger.EntityCreated("Operation", redacted, new Dictionary<string, object?>
        {
            ["CorrelationId"] = activity?.Id,
            ["Timestamp"] = DateTime.UtcNow
        });


        // Publish event to real-time event publisher
        await _eventPublisher.PublishCreatedAsync(op.Id, op.Name);

        return Result.Success(new OperationDto(op.Id, op.Name, op.Status.ToString(), op.CreatedAt, op.UpdatedAt));
    }
}

/// <summary>
/// Read model for an integration operation.
/// </summary>
public record OperationDto(Guid Id, string Name, string Status, DateTime CreatedAt, DateTime UpdatedAt);