using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.TemplateTests.Infrastructure.Common;

namespace VsaTemplate.TemplateTests;

public sealed class ServiceCollectionExtensionTests
{
    [Test]
    public void AddRequestHandlersShouldRegisterRequestHandlers()
    {
        var services = new ServiceCollection();
        services.AddRequestHandlers(typeof(ServiceCollectionExtensionTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        var handler = serviceProvider.GetService<TestRequestHandler>();
        handler.ShouldNotBeNull();
    }
}
