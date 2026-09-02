using System.Collections.ObjectModel;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Services;

namespace VsaTemplate.Tests.TestInfrastructure;

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

    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        _dispatchedEvents.Add(domainEvent);

        await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
    }
}
