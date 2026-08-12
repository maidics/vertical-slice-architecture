using System.Collections.ObjectModel;
using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Infrastructure;

namespace VsaTemplate.FunctionalTests.Infrastructure;

public sealed class DomainEventDispatcherSpy : IDomainEventDispatcher
{
    private readonly DomainEventDispatcher _dispatcher;
    private readonly List<IDomainEvent> _dispatchedEvents = [];

    public DomainEventDispatcherSpy(DomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public ReadOnlyCollection<IDomainEvent> DispatchedEvents => _dispatchedEvents.AsReadOnly();

    //this is called in [SetUp]
    public void ClearDomainEvents() => _dispatchedEvents.Clear();

    public bool HasDispatchedEventType<TEvent>()
        where TEvent : IDomainEvent
    {
        return _dispatchedEvents.OfType<TEvent>().Any();
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        _dispatchedEvents.Add(@event);

        await _dispatcher.DispatchAsync(@event, cancellationToken);
    }
}
