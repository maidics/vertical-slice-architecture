using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Common.Services;

public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : IDomainEvent;
}

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var eventType = @event.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<>.Handle));

        foreach (var handler in handlers)
        {
            var task = (Task)handleMethod!.Invoke(handler, [@event, cancellationToken])!;
            await task;
        }
    }
}
