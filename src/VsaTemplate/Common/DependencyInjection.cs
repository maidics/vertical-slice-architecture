using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Pipeline;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCommonServices()
        {
            var assembly = typeof(Program).Assembly;
            builder.Services.AddRequestHandlers(assembly);
            builder.Services.AddDomainEventHandlers(assembly);
            builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

            return builder;
        }
    }
}
