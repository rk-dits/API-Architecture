using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Messaging.Implementations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.DependencyInjection;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var host = configuration["Messaging:RabbitMQ:HostName"] ?? "localhost";
        var username = configuration["Messaging:RabbitMQ:UserName"] ?? "guest";
        var password = configuration["Messaging:RabbitMQ:Password"] ?? "guest";



        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
