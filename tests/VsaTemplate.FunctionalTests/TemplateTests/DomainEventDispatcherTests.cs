using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Services;
using VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class DomainEventDispatcherTests : TemplateTestBase
{
    [Test]
    public async Task DispatchAsyncShouldLogWarningIfNoHandlersAreRegisteredToIt()
    {
        var logger = new TemplateTestLogger<DomainEventDispatcher>();
        var dispatcher = new DomainEventDispatcher(_templateTesting.ServiceProvider, logger);

        await dispatcher.DispatchAsync(new Ping(), CancellationToken.None);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Warning);
        logger.Entries[0].Message.ShouldContain("No IDomainEventHandler registered");
    }

    [Test]
    public async Task DispatchAsyncShouldDispatchEventIfHasHandlerRegisteredToIt()
    {
        _templateTesting.Services.AddDomainEventHandlers(
            typeof(DomainEventDispatcherTests).Assembly
        );

        var logger = new TemplateTestLogger<DomainEventDispatcher>();
        var dispatcher = new DomainEventDispatcher(_templateTesting.ServiceProvider, logger);
        var spy = new TemplateTestDomainEventDispatcherSpy(dispatcher);

        var @event = new Ping();
        @event.Action = () => spy.IncrementDispatched(@event);

        await spy.DispatchAsync(@event, CancellationToken.None);
        spy.HandledCount.ShouldBe(1);
        spy.DispatchedCount.ShouldBe(1);
        spy.HasDispatchedEventType<Ping>();

        // if it did not return early because of no handlers then it surely handled the event
        logger.Entries.Count.ShouldBe(0);
    }

    [Test]
    public async Task DispatchASyncShouldDispatchEventForAllHandler()
    {
        _templateTesting.Services.AddDomainEventHandlers(
            typeof(DomainEventDispatcherTests).Assembly
        );

        var logger = new TemplateTestLogger<DomainEventDispatcher>();
        var dispatcher = new DomainEventDispatcher(_templateTesting.ServiceProvider, logger);
        var spy = new TemplateTestDomainEventDispatcherSpy(dispatcher);

        var @event = new Pong();
        @event.Action = () => spy.IncrementDispatched(@event);
        await spy.DispatchAsync(@event, CancellationToken.None);
        spy.HandledCount.ShouldBe(2);
        spy.DispatchedCount.ShouldBe(1);
        spy.HasDispatchedEventType<Pong>();

        // if it did not return early because of no handlers then it surely handled the event
        logger.Entries.Count.ShouldBe(0);
    }
}
