using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;
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

    [Test]
    public void AddDomainEventHandlersShouldRegisterDomainEventHandlers()
    {
        var services = new ServiceCollection();
        services.AddDomainEventHandlers(typeof(ServiceCollectionExtensionTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        var handler = serviceProvider.GetService<IDomainEventHandler<TestDomainEvent>>();
        handler.ShouldNotBeNull();
    }
}
