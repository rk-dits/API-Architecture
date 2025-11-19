using BuildingBlocks.Persistence.Abstractions;
using BuildingBlocks.Persistence.Ef;
using BuildingBlocks.Infrastructure.DependencyInjection;
using CoreWorkflowService.Domain.Entities;
using CoreWorkflowService.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR.Pipeline;
using CoreWorkflowService.Application.WorkflowCases;
using CoreWorkflowService.Application.Common;
using Microsoft.Extensions.Hosting;
using BuildingBlocks.Messaging.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace CoreWorkflowService.Infrastructure.DependencyInjection;

public static class CoreWorkflowServiceServiceCollectionExtensions
{
    public static IServiceCollection AddCoreWorkflowServiceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence<CoreWorkflowServiceDbContext>(configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CoreWorkflowServiceServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateWorkflowCaseCommand>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IRepository<WorkflowCase>, RepositoryBase<WorkflowCase>>();
        services.AddScoped<DbContext, CoreWorkflowServiceDbContext>();
        services.AddRabbitMqMessaging(configuration);
        services.AddHostedService<CoreWorkflowService.Infrastructure.Messaging.OutboxDispatcherHostedService>();
        return services;
    }
}
