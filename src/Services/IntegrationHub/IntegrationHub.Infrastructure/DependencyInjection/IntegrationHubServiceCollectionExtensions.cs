using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Persistence.Abstractions;
using BuildingBlocks.Persistence.Ef;
using IntegrationHub.Domain.Entities;
using IntegrationHub.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR.Pipeline;
using IntegrationHub.Application.Common;
using BuildingBlocks.Messaging.DependencyInjection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Compliance;
using IntegrationHub.Application.Operations;

namespace IntegrationHub.Infrastructure.DependencyInjection;

public static class IntegrationHubServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationHubModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence<IntegrationHubDbContext>(configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IntegrationHubServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateOperationCommand>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IRepository<Operation>, RepositoryBase<Operation>>();
        // Map base DbContext for repositories expecting DbContext rather than the concrete type
        services.AddScoped<DbContext, IntegrationHubDbContext>();
        // Add MassTransit and consumers in a single call
        var host = configuration["Messaging:RabbitMQ:HostName"] ?? "localhost";
        var username = configuration["Messaging:RabbitMQ:UserName"] ?? "guest";
        var password = configuration["Messaging:RabbitMQ:Password"] ?? "guest";
        services.AddMassTransit(x =>
        {
            x.AddConsumer<IntegrationHub.Infrastructure.Messaging.Consumers.OperationCreatedEventConsumer>();
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<BuildingBlocks.Messaging.Abstractions.IEventPublisher, BuildingBlocks.Messaging.Implementations.MassTransitEventPublisher>();
        services.AddHostedService<IntegrationHub.Infrastructure.Messaging.OutboxDispatcherHostedService>();


        // Register Redis pub/sub service
        var redisConfig = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<BuildingBlocks.Common.PubSub.IPubSubService>(
            new IntegrationHub.Infrastructure.Redis.RedisPubSubService(redisConfig));
        services.AddScoped<IntegrationHub.Application.Operations.IOperationEventPublisher, IntegrationHub.Infrastructure.Redis.RedisOperationEventPublisher>();

        return services;
    }
}