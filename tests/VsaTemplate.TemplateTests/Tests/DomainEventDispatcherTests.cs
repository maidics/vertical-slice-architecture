using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Services;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests.Tests;

public sealed class DomainEventDispatcherTests : TestBase
{
    [Test]
    public async Task DispatchAsyncShouldLogWarningIfNoHandlersAreRegisteredToDomainEvent()
    {
        var logger = new LoggerSpy<DomainEventDispatcher>();
        var dispatcher = new DomainEventDispatcher(
            new ServiceCollection().BuildServiceProvider(),
            logger
        );

        await dispatcher.DispatchAsync(new TestDomainEvent(), CancellationToken.None);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Warning);
        logger.Entries[0].Message.ShouldContain("No IDomainEventHandler registered");
    }

    [Test]
    public async Task DispatchAsyncShouldDispatchEventIfHasHandlersRegisteredToDomainEvent()
    {
        var logger = new LoggerSpy<DomainEventDispatcher>();
        var dispatcher = new DomainEventDispatcher(Fixture.ServiceScope.ServiceProvider, logger);
        var spy = new DomainEventDispatcherSpy(dispatcher);

        var domainEvent = new TestDomainEvent();
        domainEvent.Action = () => spy.IncrementDispatched(domainEvent);

        await spy.DispatchAsync(domainEvent, CancellationToken.None);
        spy.DispatchedEventCount.ShouldBe(1);
        spy.HandlersHandledCount.ShouldBe(2);
        spy.HasDispatchedEventType<TestDomainEvent>().ShouldBeTrue();

        logger.Entries.Count.ShouldBe(0);
    }
}
