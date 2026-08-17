using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Infrastructure.Database.Interceptors;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests.Tests;

public sealed class AuditableEntityInterceptorTests : TestBase
{
    [Test]
    public void InterceptorShouldBeRegisteredToDbContext()
    {
        using var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();

        var coreOptions = context
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>();
        coreOptions.ShouldNotBeNull();

        var interceptors = coreOptions.Interceptors?.ToList();
        interceptors.ShouldNotBeNull();

        var auditable = interceptors.OfType<AuditableEntityInterceptor>();
        auditable.Count().ShouldBe(1);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldUpdateCreatedAndModifiedPropertiesWhenEntityIsCreated(bool logUserIn)
    {
        using var context = _serviceProvider.GetRequiredService<TestDbContext>();

        Guid? userId = logUserIn ? Testing.LogUserIn(Guid.NewGuid(), []) : null;
        var time = _serviceProvider.GetRequiredService<TimeProvider>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();

        var nowDate = time.GetUtcNow().Date;

        created.CreatedBy.ShouldBe(userId);
        created.CreatedOn.Date.ShouldBe(nowDate);
        created.LastModifiedBy.ShouldBe(userId);
        created.LastModifiedOn.Date.ShouldBe(nowDate);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldUpdateModifiedPropertiesWhenEntityIsModified(bool logUserIn)
    {
        using var context = _serviceProvider.GetRequiredService<TestDbContext>();

        Guid? userId = logUserIn ? Testing.LogUserIn(Guid.NewGuid(), []) : null;

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();
        var createdOn = created.CreatedOn;

        entity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        var updated = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        updated.ShouldNotBeNull();
        updated.LastModifiedBy.ShouldBe(userId);
        updated.LastModifiedOn.ShouldNotBe(createdOn);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldUpdateModifiedPropertiesWhenOwnedEntityIsModified(bool logUserIn)
    {
        using var context = _serviceProvider.GetRequiredService<TestDbContext>();

        Guid? userId = logUserIn ? Testing.LogUserIn(Guid.NewGuid(), []) : null;

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();
        var createdOn = created.CreatedOn;

        entity.OwnedEntity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        var updated = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        updated.ShouldNotBeNull();
        updated.LastModifiedBy.ShouldBe(userId);
        updated.LastModifiedOn.ShouldNotBe(createdOn);
    }
}
