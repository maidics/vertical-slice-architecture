using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class PingRequestHandler : IRequestHandler
{
    public Task Handle() => Task.CompletedTask;
}

public sealed record Ping : IDomainEvent
{
    public Action Action { get; set; } = () => { };
}

public sealed class PingEventHandler : IDomainEventHandler<Ping>
{
    public Task Handle(Ping @event, CancellationToken cancellationToken)
    {
        @event.Action();
        return Task.CompletedTask;
    }
}
