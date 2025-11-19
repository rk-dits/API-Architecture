using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Persistence.Abstractions;
using IntegrationHub.Domain.Entities;
using MediatR;
using BuildingBlocks.Infrastructure.Tracing;
using System.Diagnostics;

namespace IntegrationHub.Application.Operations;

public record GetOperationQuery(Guid Id) : IRequest<Result<OperationDto>>;

public class GetOperationHandler : IRequestHandler<GetOperationQuery, Result<OperationDto>>
{
    private readonly IRepository<Operation> _repository;

    public GetOperationHandler(IRepository<Operation> repository)
    {
        _repository = repository;
    }

    public async Task<Result<OperationDto>> Handle(GetOperationQuery request, CancellationToken cancellationToken)
    {
        using var activity = PlatformActivity.Source.StartActivity("GetOperationHandler.Handle", ActivityKind.Internal);
        activity?.AddTag("operation.id", request.Id);
        var op = await _repository.GetAsync(request.Id, cancellationToken);
        if (op is null) return Result.Failure<OperationDto>(Error.NotFound("Operation", request.Id.ToString()));
        activity?.SetStatus(ActivityStatusCode.Ok);
        return Result.Success(new OperationDto(op.Id, op.Name, op.Status.ToString(), op.CreatedAt, op.UpdatedAt));
    }
}