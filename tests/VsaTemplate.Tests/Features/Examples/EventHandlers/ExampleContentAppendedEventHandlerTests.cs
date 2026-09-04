using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Services;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Domain.Events;
using VsaTemplate.Features.Examples;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.EventHandlers;

public sealed class ExampleContentAppendedEventHandlerTests : FunctionalTestBase
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
    public async Task ShouldThrowInvalidOperationExceptionWhenExampleNotFound()
    {
        var domainEvent = new ExampleContentAppendedEvent(Guid.Empty);

        var handler = GetRequiredService<IDomainEventHandler<ExampleContentAppendedEvent>>();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.Handle(domainEvent, CancellationToken.None)
        );

        ex.Message.ShouldBe($"{nameof(Example)} not found: {Guid.Empty}");
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
