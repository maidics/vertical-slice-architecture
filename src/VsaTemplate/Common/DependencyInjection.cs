using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Pipeline;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCommonServices()
        {
            builder.Services.AddRequestHandlers(typeof(Program).Assembly);

            builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

            return builder;
        }
    }
}
