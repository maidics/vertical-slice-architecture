using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class PingRequestHandler : IRequestHandler
{
    public Task Handle() => Task.CompletedTask;
}

public sealed record Ping : IDomainEvent;

public sealed class PingEventHandler : IDomainEventHandler<Ping>
{
    public Task Handle(Ping @event, CancellationToken cancellationToken) => Task.CompletedTask;
}
