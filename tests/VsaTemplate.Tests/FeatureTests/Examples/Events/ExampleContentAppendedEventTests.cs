using Shouldly;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands;
using VsaTemplate.Features.Examples.Events;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Events;

public sealed class ExampleContentAppendedEventTests : ApplicationTestBase
{
    [Test]
    public async Task AppendExampleContentShouldDispatchEvent()
    {
        var example = new Example { Content = "test" };
        await Testing.AddAsync(example);

        var command = new AppendExampleContentCommand(example.Id, "-content");
        var handler = GetService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var spy = GetService<DomainEventDispatcherSpy>();
        spy.DispatchedEvents.Count.ShouldBe(1);
        spy.HasDispatchedEventType<ExampleContentAppendedEvent>();
    }

    [Test]
    public async Task ShouldThrowIfExampleIsNotFound()
    {
        var @event = new ExampleContentAppendedEvent(Guid.Empty);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            Testing.DispatchDomainEventAsync(@event)
        );
    }

    [Test]
    public async Task ShouldUpdateHasContentAppendedFlag()
    {
        var example = new Example { Content = "test" };
        await Testing.AddAsync(example);

        var @event = new ExampleContentAppendedEvent(example.Id);
        await Testing.DispatchDomainEventAsync(@event);

        var updated = await Testing.FirstOrDefaultAsync<Example>(x => x.Id == example.Id);
        updated.ShouldNotBeNull();
        updated.HasAppendedContent.ShouldBeTrue();
    }
}
