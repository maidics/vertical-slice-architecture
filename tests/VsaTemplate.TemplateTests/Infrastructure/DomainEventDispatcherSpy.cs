using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Services;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class DomainEventDispatcherSpy : IDomainEventDispatcher
{
    private readonly DomainEventDispatcher _dispatcher;
    private readonly Dictionary<IDomainEvent, int> _handled = new();

    public DomainEventDispatcherSpy(DomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool HasDispatchedEventType<TEvent>()
        where TEvent : IDomainEvent
    {
        return _handled.Keys.OfType<TEvent>().Any();
    }

    public int DispatchedEventCount => _handled.Count;

    public int HandlersHandledCount => _handled.Values.Sum();

    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        _handled.TryAdd(domainEvent, 0);

        await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
    }

    public void IncrementDispatched(IDomainEvent domainEvent)
    {
        _handled[domainEvent]++;
    }
}
