using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Infrastructure;

namespace VsaTemplate;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            // Infrastructure
            builder.AddInfrastructureServices();

            //Features
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Services.AddRequestHandlers();
            builder.Services.AddDomainEventHandlers();

            // Web
            builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
                options.SuppressModelStateInvalidFilter = true
            );

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
        }
    }
}
