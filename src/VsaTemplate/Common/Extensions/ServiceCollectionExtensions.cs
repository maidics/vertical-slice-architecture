using System.Reflection;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Common.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRequestHandlers(Assembly assembly)
        {
            var implementationTypes = assembly
                .GetTypes()
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false }
                    && t.IsAssignableTo(typeof(IRequestHandler))
                );

            foreach (var implementationType in implementationTypes)
            {
                services.AddScoped(implementationType);
            }

            return services;
        }

        public IServiceCollection AddDomainEventHandlers(Assembly assembly)
        {
            var handlers = assembly
                .GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .SelectMany(
                    t => t.GetInterfaces(),
                    (implementation, @interface) => new { implementation, @interface }
                )
                .Where(x =>
                    x.@interface.IsGenericType
                    && x.@interface.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                );

            foreach (var match in handlers)
            {
                services.AddTransient(match.@interface, match.implementation);
            }

            return services;
        }
    }
}
