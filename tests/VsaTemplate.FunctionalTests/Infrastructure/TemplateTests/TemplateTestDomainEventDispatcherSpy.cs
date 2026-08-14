using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Services;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

// Different implementation than DomainEventDispatcherSpy to verify that all handlers handle the event
public sealed class TemplateTestDomainEventDispatcherSpy : IDomainEventDispatcher
{
    private readonly DomainEventDispatcher _dispatcher;
    private readonly Dictionary<IDomainEvent, int> _handled = new();

    public TemplateTestDomainEventDispatcherSpy(DomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool HasDispatchedEventType<TEvent>()
        where TEvent : IDomainEvent
    {
        return _handled.Keys.OfType<TEvent>().Any();
    }

    public int DispatchedCount => _handled.Count;

    public int HandledCount => _handled.Values.Sum();

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        _handled.TryAdd(@event, 0);

        await _dispatcher.DispatchAsync(@event, cancellationToken);
    }

    public void IncrementDispatched(IDomainEvent @event)
    {
        _handled[@event]++;
    }
}
