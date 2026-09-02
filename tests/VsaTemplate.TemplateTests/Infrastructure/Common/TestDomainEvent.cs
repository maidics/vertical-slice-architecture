using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed record TestDomainEvent : IDomainEvent
{
    public Action Action { get; set; } = () => { };
}

public sealed class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
{
    public Task Handle(TestDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        domainEvent.Action();
        return Task.CompletedTask;
    }
}

public sealed class OtherTestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
{
    public Task Handle(TestDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        domainEvent.Action();
        return Task.CompletedTask;
    }
}
