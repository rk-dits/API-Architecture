using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IdentityService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // Add controllers from this assembly
        services.AddControllers()
            .AddApplicationPart(Assembly.GetExecutingAssembly());

        return services;
    }
}