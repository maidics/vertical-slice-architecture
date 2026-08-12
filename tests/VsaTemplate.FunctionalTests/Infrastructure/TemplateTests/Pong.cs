using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class PongRequestHandler : IRequestHandler
{
    public Task Handle() => Task.CompletedTask;
}

public sealed record Pong : IDomainEvent;

public sealed class PongEventHandler : IDomainEventHandler<Pong>
{
    public Task Handle(Pong @event, CancellationToken cancellationToken) => Task.CompletedTask;
}
