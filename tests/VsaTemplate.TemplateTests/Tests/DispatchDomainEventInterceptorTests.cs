using Microsoft.EntityFrameworkCore.Infrastructure;
using Shouldly;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Infrastructure.Database.Interceptors;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests.Tests;

public sealed class DispatchDomainEventInterceptorTests : TestBase
{
    [Test]
    public void InterceptorShouldBeRegisteredToDbContext()
    {
        using var context = GetRequiredService<ApplicationDbContext>();

        var coreOptions = context
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>();
        coreOptions.ShouldNotBeNull();

        var interceptors = coreOptions.Interceptors?.ToList();
        interceptors.ShouldNotBeNull();

        var auditable = interceptors.OfType<DispatchDomainEventInterceptor>();
        auditable.Count().ShouldBe(1);
    }

    [Test]
    public void ShouldDispatchDomainEventWhenEntityIsCreated()
    {
        using var context = GetRequiredService<TestDbContext>();

        var spy = GetRequiredService<DomainEventDispatcherSpy>();

        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        domainEvent.Action = () => spy.IncrementDispatched(domainEvent);
        entity.AddDomainEvent(domainEvent);
        context.Add(entity);
        context.SaveChanges();

        spy.DispatchedEventCount.ShouldBe(1);
        spy.HandlersHandledCount.ShouldBe(2);
        spy.HasDispatchedEventType<TestDomainEvent>().ShouldBeTrue();
    }

    [Test]
    public void ShouldDispatchDomainEventWhenEntityIsUpdated()
    {
        using var context = GetRequiredService<TestDbContext>();

        var spy = GetRequiredService<DomainEventDispatcherSpy>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var domainEvent = new TestDomainEvent();
        domainEvent.Action = () => spy.IncrementDispatched(domainEvent);
        entity.AddDomainEvent(domainEvent);
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        spy.DispatchedEventCount.ShouldBe(1);
        spy.HandlersHandledCount.ShouldBe(2);
        spy.HasDispatchedEventType<TestDomainEvent>().ShouldBeTrue();
    }
}
