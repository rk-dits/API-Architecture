using System.Reflection;
using BuildingBlocks.Persistence.Abstractions;
using BuildingBlocks.Persistence.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Events;
using BuildingBlocks.Infrastructure.Compliance;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilogLogging(configuration)
        .AddOpenTelemetry(configuration)
        .AddComplianceServices();
        return services;
    }

    public static IServiceCollection AddPersistence<TContext>(this IServiceCollection services, IConfiguration configuration)
        where TContext : AppDbContextBase
    {
        var connectionString = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing connection string 'Default'.");
        services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
        // Wrap the DbContext in a UnitOfWork implementation so consumers depend only on the abstraction.
        services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<TContext>()));
        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        services.AddLogging(lb => lb.AddSerilog());
        return services;
    }

    private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["Service:Name"] ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "acme-service";
        // Keep minimal registration (no instrumentation extensions) until required packages are added.
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName));
        return services;
    }

    private static IServiceCollection AddComplianceServices(this IServiceCollection services)
    {
        services.AddSingleton<IRedactionService, ReflectionRedactionService>();
        services.AddSingleton<IAuditLogger, SerilogAuditLogger>();
        return services;
    }
}
