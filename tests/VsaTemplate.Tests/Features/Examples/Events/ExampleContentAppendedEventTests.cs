using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Services;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands.AppendContent;
using VsaTemplate.Features.Examples.Events;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.Events;

public sealed class ExampleContentAppendedEventTests : FunctionalTestBase
{
    [Test]
    public async Task AppendExampleContentShouldDispatchEvent()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new AppendExampleContentCommand(example.Id, "-content");
        var handler = GetRequiredService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var spy = GetRequiredService<DomainEventDispatcherSpy>();
        spy.DispatchedEvents.Count.ShouldBe(1);
        spy.HasDispatchedEventType<ExampleContentAppendedEvent>().ShouldBeTrue();
    }

    [Test]
    public async Task ShouldThrowIfExampleIsNotFound()
    {
        var domainEvent = new ExampleContentAppendedEvent(Guid.Empty);

        var dispatcher = GetRequiredService<IDomainEventDispatcher>();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync(domainEvent, CancellationToken.None)
        );
    }

    [Test]
    public async Task ShouldUpdateHasContentAppendedFlag()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var domainEvent = new ExampleContentAppendedEvent(example.Id);
        var dispatcher = GetRequiredService<IDomainEventDispatcher>();
        await dispatcher.DispatchAsync(domainEvent, CancellationToken.None);

        var updated = await context.Examples.FirstOrDefaultAsync(x => x.Id == example.Id);
        updated.ShouldNotBeNull();
        updated.HasAppendedContent.ShouldBeTrue();
    }
}
