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
    }
}
