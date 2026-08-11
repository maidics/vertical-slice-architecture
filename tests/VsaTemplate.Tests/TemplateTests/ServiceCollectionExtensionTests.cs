using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class ServiceCollectionExtensionTests : TemplateTestBase
{
    [Test]
    public void AddRequestHandlersShouldRegisterRequestHandlers()
    {
        _templateTesting.Services.AddRequestHandlers(
            typeof(ServiceCollectionExtensionTests).Assembly
        );

        var pingHandler = _templateTesting.ServiceProvider.GetService<PingRequestHandler>();
        pingHandler.ShouldNotBeNull();
        var pongHandler = _templateTesting.ServiceProvider.GetRequiredService<PongRequestHandler>();
        pongHandler.ShouldNotBeNull();
    }

    [Test]
    public void AddDomainEventHandlersShouldRegisterDomainEventHandlers()
    {
        _templateTesting.Services.AddDomainEventHandlers(
            typeof(ServiceCollectionExtensionTests).Assembly
        );

        var pingHandler = _templateTesting.ServiceProvider.GetService<IDomainEventHandler<Ping>>();
        pingHandler.ShouldNotBeNull();
        var pongHandler = _templateTesting.ServiceProvider.GetService<IDomainEventHandler<Pong>>();
        pongHandler.ShouldNotBeNull();
    }
}
