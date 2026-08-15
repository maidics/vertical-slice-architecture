using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class PongRequestHandler : IRequestHandler
{
    public Task Handle() => Task.CompletedTask;
}

public sealed record Pong : IDomainEvent
{
    public Action Action { get; set; } = () => { };
}

public sealed class PongEventHandler : IDomainEventHandler<Pong>
{
    public Task Handle(Pong domainEvent, CancellationToken cancellationToken)
    {
        domainEvent.Action();
        return Task.CompletedTask;
    }
}

public sealed class OtherPongEventHandler : IDomainEventHandler<Pong>
{
    public Task Handle(Pong domainEvent, CancellationToken cancellationToken)
    {
        domainEvent.Action();
        return Task.CompletedTask;
    }
}
